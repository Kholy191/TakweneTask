using Microsoft.AspNetCore.Authorization;

namespace Takwene.API.Authorization;

// Requires a "permission" claim; each permission is registered as a policy of the same name.
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission);
