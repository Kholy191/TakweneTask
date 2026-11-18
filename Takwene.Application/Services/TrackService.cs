using Takwene.Application.Common;
using Takwene.Application.Common.Errors;
using Takwene.Application.DTOs.Tracks;
using Takwene.Application.Interfaces;
using Takwene.Application.Specifications.Tracks;
using Takwene.Domain.Entities;
using Takwene.Domain.Enums;
using Takwene.Domain.Rules;

namespace Takwene.Application.Services;

internal sealed class TrackService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : ITrackService
{
    public async Task<Result<TrackDetailsDto>> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default)
    {
        var artistId = await ResolveOwnerAsync(request.ArtistId, cancellationToken);
        if (artistId.IsFailure)
            return artistId.Error;

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
            ArtistId = artistId.Value,
            Status = TrackStatus.Draft
        };

        await tracks.AddAsync(track, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return await GetByIdAsync(track.Id, cancellationToken);
    }

    public async Task<Result<PagedResult<TrackListItemDto>>> GetAllAsync(TrackQueryParameters query, CancellationToken cancellationToken = default)
    {
        var scope = ArtistScope.For(currentUser);
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
        var scope = ArtistScope.For(currentUser);
        if (scope.IsFailure)
            return scope.Error;

        // Another artist's track is reported as not found, so ids can't be probed.
        var track = await unitOfWork.Repository<Track>()
            .FirstOrDefaultAsync(new TrackDetailsSpecification(id, scope.Value), cancellationToken);

        return track is null ? TrackErrors.NotFound : track;
    }

    public async Task<Result> UpdateStatusAsync(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default)
    {
        var scope = ArtistScope.For(currentUser);
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

    // Admins must name an existing artist; artists always own what they create (any ArtistId sent is ignored).
    private async Task<Result<Guid>> ResolveOwnerAsync(Guid? requestedArtistId, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return currentUser.ArtistId is { } ownArtistId
                ? ownArtistId
                : CommonErrors.ArtistProfileMissing;
        }

        if (requestedArtistId is not { } artistId)
            return TrackErrors.ArtistIdRequired;

        if (await unitOfWork.Repository<Artist>().GetByIdAsync(artistId, cancellationToken) is null)
            return ArtistErrors.NotFound;

        return artistId;
    }
}
