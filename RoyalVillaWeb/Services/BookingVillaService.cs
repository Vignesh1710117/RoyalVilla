using RoyalVilla_API.Models;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Services
{
    public class BookingVillaService : BaseService, IBookingVilla
    {
        private const string APIEndpoint = $"/api/Villa/Book/";

        public BookingVillaService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {

        }

        public Task<T?> BookVilla<T>(int id, BookingVillaDTO bookingVillaDTO)
        {
            return SendAsync<T>(
                new ApiRequest
                {
                    ApiType = SD.ApiType.POST,
                    Data = bookingVillaDTO,
                    Url = APIEndpoint + id

            });
        }
    }
}
