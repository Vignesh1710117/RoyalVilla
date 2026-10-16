using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Services
{
    public class VillaAmenitiesService : BaseService, IVillaAmenitiesService
    {

        private const string APIEndpoint = $"/api/{SD.CurrentApiVersion}/Villa-amenities/villa/";  

        public VillaAmenitiesService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {
            
        }

        public Task<T?> GetAmenityAsync<T>(int id)
        {
            return SendAsync<T>(
                new ApiRequest
                {
                    ApiType = SD.ApiType.GET,
                    Data = id,
                    Url = APIEndpoint + id

           });
        }
    }
}
