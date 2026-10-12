using System.Security.Claims;

namespace RoyalVillaWeb.Services.IService
{
    public interface ITokenProvider
    {
        void SetToken(string acesssToken, string refreshToken);
        string? GetAccessToken();
        string? GetRefreshToken();
        void ClearToken();

        ClaimsPrincipal? CreatePrincipleFromJwtToken(string? token);

    }
}
