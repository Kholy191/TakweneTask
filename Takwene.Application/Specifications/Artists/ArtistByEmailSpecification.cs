using Takwene.Domain.Entities;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Specifications.Artists;

public sealed class ArtistByEmailSpecification(string normalizedEmail)
    : BaseSpecification<Artist>(a => a.Email == normalizedEmail);
