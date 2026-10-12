using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services.IServices;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

namespace RoyalVilla_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AuthService(ApplicationDbContext db, IMapper mapper, IConfiguration configuration, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> usermanager, ITokenService tokenService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _roleManager = roleManager;
            _userManager = usermanager;
            _tokenService = tokenService;
        }
        public async Task<bool> IsEmailExist(string email)
        {
            //await _userManager.FindByEmailAsync(email); // helper is there in identity or use below
            return await _db.ApplicationUsers.AnyAsync(u=>u.Email.ToLower() == email.ToLower());
        }


        public async Task<UserDTO> RegisterAsync(RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {

                if (await IsEmailExist(registerationRequestDTO.Email))
                {
                    throw new InvalidOperationException($"User with email '{registerationRequestDTO.Email}' already exists");
                }
                
                ApplicationUser user = new()
                {
                    Email = registerationRequestDTO.Email,
                    Name = registerationRequestDTO.Name,
                    NormalizedEmail = registerationRequestDTO.Email,
                    UserName = registerationRequestDTO.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);

                if(!result.Succeeded)
                {
                    var error = string.Join(", ", result.Errors.Select(e=>e.Description));
                    throw new InvalidOperationException($"User registeration failed {error} ");
                }



                var Role = string.IsNullOrEmpty(registerationRequestDTO.Role) ? "Customer" : registerationRequestDTO.Role;

                if(!await _roleManager.RoleExistsAsync(Role))
                {
                   await _roleManager.CreateAsync(new IdentityRole(Role));
                }

                await _userManager.AddToRoleAsync(user, Role);

                //User user = _mapper.Map<User>(registerationRequestDTO);

                //await _db.Users.AddAsync(user);
                //await _db.SaveChangesAsync();   
                
                var userDto = _mapper.Map<UserDTO>(user);
                userDto.Role = Role;

                return userDto;

            }
            catch (Exception ex) {
                throw new InvalidOperationException($"An unexpected error occurred during user registration{ex.Message}");
            }
        
        }

        public async Task<TokenDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u=>u.Email == loginRequestDTO.Email);

                if(user == null)
                {
                    return null; // invalid user
                }

                bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);


                if(!isValid)
                {
                    return null; //invalid password
                }

                var token = await _tokenService.GenerateJwtTokenAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var jwtTokenId = jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti)?.Value;
                //genrete new refreshToken
                var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync();
                
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await _tokenService.SaveRefreshToken(user.Id, jwtTokenId, newRefreshToken, refreshTokenExpiry);

                TokenDTO tokenDTO =  new TokenDTO
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo
                };

                return tokenDTO;

             
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred during Login", ex);
            }

        }

        public async Task<TokenDTO> RefreshTokenRequestAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshTokenRequestDTO.RefreshToken)) {
                    return null;
                }

                //validate refresh token
                var (Isvalid, userId, TokenfamilyId, TokenReused) = await _tokenService.ValidateRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);
                
                //token reused detection
                if (TokenReused)
                {
                    return null;
                }

                //token invalid or expired
                if(!Isvalid || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(TokenfamilyId))
                {
                    return null;
                }

                //find user

                var user = await _db.ApplicationUsers.FindAsync(userId);

                if(user == null)
                {
                    return null;
                }
                //revoke old refresh token

                await _tokenService.RevokeRefreshToken(refreshTokenRequestDTO.RefreshToken);

                //genreate new accesstoken & refreshtoken

                var token = await _tokenService.GenerateJwtTokenAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                //genrete new refreshToken
                var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync();

                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await _tokenService.SaveRefreshToken(user.Id, TokenfamilyId, newRefreshToken, refreshTokenExpiry);

                TokenDTO tokenDTO = new TokenDTO
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo
                };

                return tokenDTO;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred during token refresh", ex);
            }

        }
    }
}
