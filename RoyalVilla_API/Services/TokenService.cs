using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RoyalVilla_API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, UserManager<ApplicationUser> usermanager, ApplicationDbContext db, ILogger<TokenService> logger)
        {
            _db = db;
            _configuration = configuration;
            _userManager = usermanager;
            _logger = logger;
        }

        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);

            var roles = await _userManager.GetRolesAsync(user);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Email,user.Email),
                    new Claim(ClaimTypes.Name,user.Name),
                    new Claim(ClaimTypes.Role,roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };


            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            _logger.LogInformation($"JWT token generated for user '{user.Email}' with expiration at {tokenDescriptor.Expires} UTC.");
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            var refreshToken = Convert.ToBase64String(randomNumber);


            var exist = await _db.RefreshTokens.AnyAsync(u => u.RefreshTokenValue == refreshToken);
            if (exist)
            {
                _logger.LogWarning("Generated refresh token already exists. Generating a new one.");
                return await GenerateRefreshTokenAsync();
            }

            _logger.LogInformation("Refresh token generated successfully.");
            return refreshToken;
        }

        public async Task<bool> RevokeRefreshToken(string refreshTokenId)
        {
            var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(u=>u.RefreshTokenValue == refreshTokenId);

            if(storedToken == null)
            {
                _logger.LogWarning("Refresh token not found.");
                return false;
            }

            storedToken.IsValid = false;
            await _db.SaveChangesAsync();
            return true;

        }

        public async Task SaveRefreshToken(string userId, string JwtTokenId, string refreshToken, DateTime expiresAt)
        {
            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                JwtTokenId = JwtTokenId,
                ExpiresAt = expiresAt,
                RefreshTokenValue = refreshToken,
                IsValid = true
            };
            
            _logger.LogInformation($"Saving refresh token for user '{userId}' with expiration at {expiresAt} UTC.");    
            await _db.RefreshTokens.AddAsync(refreshTokenEntity);
            await _db.SaveChangesAsync();
        }

        public async Task<(bool Isvalid, string? userId, string? TokenfamilyId, bool TokenReused)> ValidateRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(u => u.RefreshTokenValue == refreshToken);

            //Token doesn't exit
            if (storedToken == null)
            {
                _logger.LogWarning("Refresh token not found.");
                return (false,null, null, false);
            }

                //token reuse detection
                //if token exist but it is invalid then someone tried to use it
                //indicator of token theft
            if (!storedToken.IsValid)
            {
                //revoke all token

                var tokenfamily = await _db.RefreshTokens.Where(u => u.JwtTokenId == storedToken.JwtTokenId && u.UserId == storedToken.UserId).ToListAsync();
                
                if (tokenfamily.Count > 0)
                {
                    foreach (var token in tokenfamily)
                    {
                        token.IsValid = false;
                    }

                   await _db.SaveChangesAsync();
                }
                _logger.LogWarning($"Refresh token reuse detected for user '{storedToken.UserId}'. All tokens in the family have been revoked.");
                return (false, storedToken.UserId, storedToken.JwtTokenId, true);

                
            }
            if(storedToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning($"Refresh token for user '{storedToken.UserId}' has expired.");
                return (false, storedToken.UserId, storedToken.JwtTokenId, false);
            }

            _logger.LogInformation($"Refresh token for user '{storedToken.UserId}' is valid.");
            return (true, storedToken.UserId, storedToken.JwtTokenId, false);
        }
    }
}
