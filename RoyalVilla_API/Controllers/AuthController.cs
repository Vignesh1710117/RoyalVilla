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
        public AuthController(IAuthService authService)
        {
           _authService = authService;
            
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {
                if (registerationRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registeration data is required"));
                }
                if (await _authService.IsEmailExist(registerationRequestDTO.Email))
                {

                    return Conflict(ApiResponse<object>.Conflict($"User with email '{registerationRequestDTO.Email}' already exists"));
                }

                var user = await _authService.RegisterAsync(registerationRequestDTO);


                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registeration is failed"));
                }

                var response = ApiResponse<UserDTO>.CreatedAt(user, "User registered successfully");

                return Ok(response);
            }
            catch(Exception ex) 
            {
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
                return BadRequest(ApiResponse<object>.BadRequest("Login data is required"));
            }

            var loginResponse = await _authService.LoginAsync(loginRequestDTO);

            if (loginResponse == null)
            {
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
                    return BadRequest(ApiResponse<object>.BadRequest("refresh token is required"));
                }

                var tokenResponse = await _authService.RefreshTokenRequestAsync(refreshTokenRequestDTO);

                if(tokenResponse == null)
                {
                    var errorResponse = ApiResponse<object>.Error(401, "Invalid or expired token.");
                    return Unauthorized(errorResponse);
                }

                var response = ApiResponse<TokenDTO>.Ok(tokenResponse, "Token refreshed successfully");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorMessage = ApiResponse<object>.Error(500, "Error occurred during token creation", ex.Message);
                return StatusCode(500, errorMessage); 
            }
        }
    }
}
