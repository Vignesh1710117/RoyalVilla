using RoyalVilla.DTO;

namespace RoyalVilla_API.Services.IServices
{
    public interface IAuthService
    {
        Task<UserDTO> RegisterAsync(RegisterationRequestDTO registerationRequestDTO);
        Task<TokenDTO> LoginAsync(LoginRequestDTO loginRequestDTO);

        Task<bool> IsEmailExist(string email);
        Task<TokenDTO> RefreshTokenRequestAsync(RefreshTokenRequestDTO refreshTokenRequestDTO);
    }   
}
