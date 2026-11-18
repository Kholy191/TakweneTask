using Takwene.Application.Artists.Specifications;
using Takwene.Application.Common;
using Takwene.Application.Interfaces;
using Takwene.Domain.Entities;

namespace Takwene.Application.Artists;

public interface IArtistService
{
    Task<Result<ArtistCreatedResponse>> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<ArtistDto>> GetAllAsync(PagedQuery query, CancellationToken cancellationToken = default);
}

internal sealed class ArtistService(IUnitOfWork unitOfWork, IAuthService authService) : IArtistService
{
    public async Task<Result<ArtistCreatedResponse>> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default)
    {
        var artists = unitOfWork.Repository<Artist>();
        var email = request.Email.Trim().ToLowerInvariant();

        // Fail early, before opening a transaction.
        if (await artists.AnyAsync(new ArtistByEmailSpecification(email), cancellationToken))
            return ArtistErrors.EmailAlreadyExists;

        var password = request.Password ?? PasswordGenerator.Generate();

        // User and artist are created together or not at all.
        return await unitOfWork.ExecuteInTransactionAsync<Result<ArtistCreatedResponse>>(async () =>
        {
            var userResult = await authService.CreateUserAsync(request.Name.Trim(), email, password, Roles.Artist, cancellationToken);
            if (userResult.IsFailure)
                return userResult.Error;

            var artist = new Artist
            {
                Name = request.Name.Trim(),
                Email = email,
                Country = request.Country.Trim(),
                UserId = userResult.Value
            };

            await artists.AddAsync(artist, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            var temporaryPassword = request.Password is null ? password : null;
            return new ArtistCreatedResponse(artist.Id, artist.Name, artist.Email, artist.Country, userResult.Value, temporaryPassword);
        }, cancellationToken);
    }

    public async Task<PagedResult<ArtistDto>> GetAllAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var artists = unitOfWork.Repository<Artist>();
        var spec = new ArtistListSpecification(query.Page, query.PageSize);

        var items = await artists.ListAsync(spec, cancellationToken);
        var total = await artists.CountAsync(spec, cancellationToken);

        return new PagedResult<ArtistDto>(items, query.Page, query.PageSize, total);
    }
}
