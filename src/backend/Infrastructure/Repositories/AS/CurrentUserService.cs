using Application.IServices.AS;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Repositories.AS;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return null;
            }

            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }

            return null;
        }
    }
}
