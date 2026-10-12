using RoyalVilla_API.Models;
using System.Diagnostics.Eventing.Reader;

namespace RoyalVilla_API.Services.IServices
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync();

        Task SaveRefreshToken(string userId, string JwtTokenId, string refreshToken, DateTime expiresAt);
        Task<bool> RevokeRefreshToken(string refreshTokenId);
        Task<(bool Isvalid,string? userId, string? TokenfamilyId, bool TokenReused)> ValidateRefreshTokenAsync(string refreshToken);


    }
}
