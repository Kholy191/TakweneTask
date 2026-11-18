using Takwene.Application.Common;
using Takwene.Application.Common.Errors;
using Takwene.Application.DTOs.Tracks;
using Takwene.Application.Interfaces;
using Takwene.Application.Specifications.Dsps;
using Takwene.Application.Specifications.Tracks;
using Takwene.Domain.Entities;
using Takwene.Domain.Enums;
using Takwene.Domain.Rules;

namespace Takwene.Application.Services;

internal sealed class TrackDistributionService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : ITrackDistributionService
{
    public async Task<Result<DistributeTrackResponse>> DistributeAsync(Guid trackId, DistributeTrackRequest request, CancellationToken cancellationToken = default)
    {
        var scope = ArtistScope.For(currentUser);
        if (scope.IsFailure)
            return scope.Error;

        var track = await unitOfWork.Repository<Track>()
            .FirstOrDefaultAsync(new TrackForUpdateSpecification(trackId, scope.Value), cancellationToken);
        if (track is null)
            return TrackErrors.NotFound;

        var dspIds = request.DspIds.Distinct().ToList();
        var existingDspIds = await unitOfWork.Repository<Dsp>().ListAsync(new DspIdsSpecification(dspIds), cancellationToken);

        var unknownDspIds = dspIds.Except(existingDspIds).ToList();
        if (unknownDspIds.Count > 0)
            return DistributionErrors.UnknownDsps(unknownDspIds);

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

    public async Task<Result> UpdateDistributionStatusAsync(Guid trackId, Guid distributionId, UpdateDistributionStatusRequest request, CancellationToken cancellationToken = default)
    {
        var scope = ArtistScope.For(currentUser);
        if (scope.IsFailure)
            return scope.Error;

        var distribution = await unitOfWork.Repository<TrackDistribution>()
            .FirstOrDefaultAsync(new DistributionForUpdateSpecification(trackId, distributionId, scope.Value), cancellationToken);
        if (distribution is null)
            return DistributionErrors.NotFound;

        if (!DistributionStatusRules.CanChange(distribution.Status, request.Status))
            return DistributionErrors.InvalidStatusTransition(distribution.Status, request.Status);

        distribution.Status = request.Status;
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
