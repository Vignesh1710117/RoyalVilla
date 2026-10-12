using Microsoft.AspNetCore.Authentication.Cookies;
using RoyalVillaWeb.Services.IService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoyalVillaWeb.Services
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

   
        public ClaimsPrincipal? CreatePrincipleFromJwtToken(string? token)
         {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                var emailClaims = jwt.Claims.FirstOrDefault(u => u.Type == "email");

                if (emailClaims != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, emailClaims.Value));
                }
                var roleClaims = jwt.Claims.FirstOrDefault(u => u.Type == "role");

                if (roleClaims != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleClaims.Value));
                }

                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public string? GetAccessToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(SD.SessionAccessToken);
        }
        public string? GetRefreshToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(SD.SessionRefreshToken);
        }

        public void SetToken(string accessToken, string refreshToken)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(SD.SessionAccessToken, accessToken);
            _httpContextAccessor.HttpContext?.Session.SetString(SD.SessionRefreshToken, refreshToken);
        }

        public void ClearToken()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(SD.SessionAccessToken);
            _httpContextAccessor.HttpContext?.Session.Remove(SD.SessionRefreshToken);
        }

    }
}
