using RoyalVilla.DTO;
using RoyalVillaWeb.Extension;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Services
{
    public class VillaService : BaseService, IVillaService
    {
        private const string APIEndpoint = $"/api/{SD.CurrentApiVersion}/villa";
        public VillaService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, tokenProvider,httpContextAccessor)
        {
        }

        public Task<T?> CreateAsync<T>(VillaCreateDTO dto)
        {
            var formData = dto.ToMultipartFormData();

            return SendAsync<T>(new ApiRequest 
            {
                ApiType = SD.ApiType.POST,
                Data = formData,
                Url = APIEndpoint

            });
        }

        public Task<T?> DeleteAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{APIEndpoint}/{id}"

            });
        }

        public Task<T?> GetAllAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = APIEndpoint

            });
        }

        public Task<T?> GetAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Data = id,
                Url = $"{APIEndpoint}/{id}"

            });
        }

        public Task<T?> UpdateAsync<T>(VillaUpdateDTO dto)
        {
            var formData = dto.ToMultipartFormData();
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = formData,
                Url = $"{APIEndpoint}/{dto.Id}"

            });
        }
    }
}
