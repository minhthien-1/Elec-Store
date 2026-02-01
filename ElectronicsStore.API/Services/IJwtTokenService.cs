using System.Security.Claims;

namespace ElectronicsStore.API.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(int userId, string email);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
