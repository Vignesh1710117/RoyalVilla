using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Services
{
    public class AuthService :BaseService, IAuthService
    {
        public readonly IHttpClientFactory _HttpClientFactory;
        private readonly string _VillaUrl;
        private const string APIEndpoint = "/api/auth";
        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {

        }
        public Task<T?> LoginAsync<T>(LoginRequestDTO loginRequestDTO)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = loginRequestDTO,
                Url = APIEndpoint  + "/login"

            }, withBearer:false);
        }

        public Task<T?> RegisterAsync<T>(RegisterationRequestDTO registerationRequestDTO)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = registerationRequestDTO,
                Url = APIEndpoint + "/register"

            }, withBearer:false);
        }

        public Task<T?> RefreshTokenAsync<T>(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = refreshTokenRequestDTO,
                Url = APIEndpoint + "/refresh-token"

            }, withBearer: false);
        }
    }
}
