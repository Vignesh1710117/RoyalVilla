using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Controllers
{
    [Route("api/auth")]
    [ApiVersionNeutral]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
           _authService = authService;
           _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {
                if (registerationRequestDTO == null)
                {
                    _logger.LogError("Registeration data is required");
                    return BadRequest(ApiResponse<object>.BadRequest("Registeration data is required"));
                }
                if (await _authService.IsEmailExist(registerationRequestDTO.Email))
                {
                    _logger.LogError($"User with email '{registerationRequestDTO.Email}' already exists");
                    return Conflict(ApiResponse<object>.Conflict($"User with email '{registerationRequestDTO.Email}' already exists"));
                }

                var user = await _authService.RegisterAsync(registerationRequestDTO);


                if (user == null)
                {
                    _logger.LogError("Registeration is failed, User doesnt exist");
                    return BadRequest(ApiResponse<object>.BadRequest("Registeration is failed, User doesnt exist"));
                }


                _logger.LogInformation("User registered successfully", user.Email, user.Role, user);
                var response = ApiResponse<UserDTO>.CreatedAt(user, "User registered successfully");

                return Ok(response);
            }
            catch(Exception ex) 
            {
                _logger.LogError("Registeration Failed: " + "Passwords must be at least 6 characters., " + "Passwords must have at least one non alphanumeric character., " + "Passwords must have at least one digit ('0'-'9')., " + "Passwords must have at least one uppercase ('A'-'Z'). ");
                return BadRequest(ApiResponse<object>.BadRequest("Registeration Failed: " +
                    "Passwords must be at least 6 characters., " +
                    "Passwords must have at least one non alphanumeric character., " +
                    "Passwords must have at least one digit ('0'-'9')., " +
                    "Passwords must have at least one uppercase ('A'-'Z'). "));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenDTO>>> Login(LoginRequestDTO loginRequestDTO)
        {
            if (loginRequestDTO == null)
            {
                _logger.LogError("Login data is required");
                return BadRequest(ApiResponse<object>.BadRequest("Login data is required"));
            }

            var loginResponse = await _authService.LoginAsync(loginRequestDTO);

            if (loginResponse == null)
            {
                _logger.LogError("Login is failed. Please check credendial");
                return BadRequest(ApiResponse<object>.BadRequest("Login is failed. Please check credendial"));
            }

            var response = ApiResponse<TokenDTO>.Ok(loginResponse, "User registered successfully");

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> RefreshAccessToken(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {

            try
            {
                if (refreshTokenRequestDTO == null)
                {
                    _logger.LogError("refresh token is required");
                    return BadRequest(ApiResponse<object>.BadRequest("refresh token is required"));
                }

                var tokenResponse = await _authService.RefreshTokenRequestAsync(refreshTokenRequestDTO);

                if(tokenResponse == null)
                {
                    _logger.LogError("Invalid or expired token.", 401);
                    var errorResponse = ApiResponse<object>.Error(401, "Invalid or expired token.");
                    return Unauthorized(errorResponse);
                }

                _logger.LogInformation("Token refreshed successfully", tokenResponse);
                var response = ApiResponse<TokenDTO>.Ok(tokenResponse, "Token refreshed successfully");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during token creation {ex.Message}");
                var errorMessage = ApiResponse<object>.Error(500, "Error occurred during token creation", ex.Message);
                return StatusCode(500, errorMessage); 
            }
        }
    }
}
