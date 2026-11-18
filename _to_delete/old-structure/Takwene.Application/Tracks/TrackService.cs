using Takwene.Application.Artists;
using Takwene.Application.Common;
using Takwene.Application.Dsps;
using Takwene.Application.Interfaces;
using Takwene.Application.Tracks.Specifications;
using Takwene.Domain.Entities;
using Takwene.Domain.Enums;
using Takwene.Domain.Rules;

namespace Takwene.Application.Tracks;

public interface ITrackService
{
    Task<Result<TrackDetailsDto>> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<TrackListItemDto>>> GetAllAsync(TrackQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DistributeTrackResponse>> DistributeAsync(Guid id, DistributeTrackRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateStatusAsync(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateDistributionStatusAsync(Guid id, Guid distributionId, UpdateDistributionStatusRequest request, CancellationToken cancellationToken = default);
}

internal sealed class TrackService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : ITrackService
{
    public async Task<Result<TrackDetailsDto>> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default)
    {
        Guid artistId;

        if (currentUser.IsAdmin)
        {
            if (request.ArtistId is not { } requestedArtistId)
                return TrackErrors.ArtistIdRequired;

            if (await unitOfWork.Repository<Artist>().GetByIdAsync(requestedArtistId, cancellationToken) is null)
                return ArtistErrors.NotFound;

            artistId = requestedArtistId;
        }
        else
        {
            // Artists always create for themselves; any ArtistId in the request is ignored.
            if (currentUser.ArtistId is not { } ownArtistId)
                return CommonErrors.ArtistProfileMissing;

            artistId = ownArtistId;
        }

        var tracks = unitOfWork.Repository<Track>();
        var isrc = Isrc.Normalize(request.Isrc);

        if (await tracks.AnyAsync(new TrackByIsrcSpecification(isrc), cancellationToken))
            return TrackErrors.IsrcAlreadyExists;

        var track = new Track
        {
            Title = request.Title.Trim(),
            Isrc = isrc,
            Genre = request.Genre.Trim(),
            ReleaseDate = request.ReleaseDate,
            ArtistId = artistId,
            Status = TrackStatus.Draft
        };

        await tracks.AddAsync(track, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return await GetByIdAsync(track.Id, cancellationToken);
    }

    public async Task<Result<PagedResult<TrackListItemDto>>> GetAllAsync(TrackQueryParameters query, CancellationToken cancellationToken = default)
    {
        var scope = GetArtistScope();
        if (scope.IsFailure)
            return scope.Error;

        // Artists are always pinned to their own id; admins may filter by any artist.
        var artistId = scope.Value ?? query.ArtistId;

        var tracks = unitOfWork.Repository<Track>();
        var spec = new TrackListSpecification(artistId, query.Genre, query.Status, query.Search, query.Page, query.PageSize);

        var items = await tracks.ListAsync(spec, cancellationToken);
        var total = await tracks.CountAsync(spec, cancellationToken);

        return new PagedResult<TrackListItemDto>(items, query.Page, query.PageSize, total);
    }

    public async Task<Result<TrackDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var scope = GetArtistScope();
        if (scope.IsFailure)
            return scope.Error;

        // Another artist's track is reported as not found, so ids can't be probed.
        var track = await unitOfWork.Repository<Track>()
            .FirstOrDefaultAsync(new TrackDetailsSpecification(id, scope.Value), cancellationToken);

        return track is null ? TrackErrors.NotFound : track;
    }

    public async Task<Result<DistributeTrackResponse>> DistributeAsync(Guid id, DistributeTrackRequest request, CancellationToken cancellationToken = default)
    {
        var scope = GetArtistScope();
        if (scope.IsFailure)
            return scope.Error;

        var track = await unitOfWork.Repository<Track>()
            .FirstOrDefaultAsync(new TrackForUpdateSpecification(id, scope.Value), cancellationToken);
        if (track is null)
            return TrackErrors.NotFound;

        var dspIds = request.DspIds.Distinct().ToList();
        var existingDspIds = await unitOfWork.Repository<Dsp>().ListAsync(new DspIdsSpecification(dspIds), cancellationToken);

        var unknownDspIds = dspIds.Except(existingDspIds).ToList();
        if (unknownDspIds.Count > 0)
            return TrackErrors.UnknownDsps(unknownDspIds);

        var distributions = unitOfWork.Repository<TrackDistribution>();
        var now = DateTime.UtcNow;
        List<Guid> submitted = [], resubmitted = [], skipped = [];

        foreach (var dspId in dspIds)
        {
            var existing = track.Distributions.FirstOrDefault(d => d.DspId == dspId);

            if (existing is null)
            {
                await distributions.AddAsync(new TrackDistribution
                {
                    TrackId = track.Id,
                    DspId = dspId,
                    Status = DistributionStatus.Pending,
                    SubmittedAt = now
                }, cancellationToken);
                submitted.Add(dspId);
            }
            else if (existing.Status == DistributionStatus.Rejected)
            {
                // The (TrackId, DspId) row is unique, so a rejected one is reused for the new attempt.
                existing.Status = DistributionStatus.Pending;
                existing.SubmittedAt = now;
                resubmitted.Add(dspId);
            }
            else
            {
                // Already pending or live on this DSP.
                skipped.Add(dspId);
            }
        }

        if (track.Status == TrackStatus.Draft && submitted.Count + resubmitted.Count > 0)
            track.Status = TrackStatus.Submitted;

        await unitOfWork.CompleteAsync(cancellationToken);

        return new DistributeTrackResponse(track.Id, track.Status, submitted, resubmitted, skipped);
    }

    public async Task<Result> UpdateStatusAsync(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default)
    {
        var scope = GetArtistScope();
        if (scope.IsFailure)
            return scope.Error;

        var track = await unitOfWork.Repository<Track>()
            .FirstOrDefaultAsync(new TrackForUpdateSpecification(id, scope.Value), cancellationToken);
        if (track is null)
            return TrackErrors.NotFound;

        if (track.Status == request.Status)
            return TrackErrors.StatusUnchanged;

        var hasLiveDistribution = track.Distributions.Any(d => d.Status == DistributionStatus.Live);
        if (!TrackStatusRules.CanChange(track.Status, request.Status, hasLiveDistribution))
            return TrackErrors.InvalidStatusTransition(track.Status, request.Status);

        // Withdrawing to Draft cancels the open (pending or rejected) submissions.
        if (request.Status == TrackStatus.Draft)
        {
            var distributions = unitOfWork.Repository<TrackDistribution>();
            foreach (var distribution in track.Distributions.ToList())
                distributions.Delete(distribution);
        }

        track.Status = request.Status;
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateDistributionStatusAsync(Guid id, Guid distributionId, UpdateDistributionStatusRequest request, CancellationToken cancellationToken = default)
    {
        var scope = GetArtistScope();
        if (scope.IsFailure)
            return scope.Error;

        var distribution = await unitOfWork.Repository<TrackDistribution>()
            .FirstOrDefaultAsync(new DistributionForUpdateSpecification(id, distributionId, scope.Value), cancellationToken);
        if (distribution is null)
            return TrackErrors.DistributionNotFound;

        if (!DistributionStatusRules.CanChange(distribution.Status, request.Status))
            return TrackErrors.InvalidDistributionTransition(distribution.Status, request.Status);

        distribution.Status = request.Status;
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    // Null value = admin (no restriction). Non-admins without a linked artist are denied, never unscoped.
    private Result<Guid?> GetArtistScope()
    {
        if (currentUser.IsAdmin)
            return Result.Success<Guid?>(null);

        return currentUser.ArtistId is { } artistId
            ? Result.Success<Guid?>(artistId)
            : Result.Failure<Guid?>(CommonErrors.ArtistProfileMissing);
    }
}
