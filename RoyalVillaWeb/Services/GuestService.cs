using RoyalVilla_API.Models;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Services
{
    public class GuestService :  BaseService, IGuest
    {

        public GuestService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {
            
        }
        public Task<T?> AddingGuest<T>(string BookVillaId, AddBookingGuestsDTO request)
        {
            return SendAsync<T>(
                 new ApiRequest
                 {
                     Url = "api/guests/" + BookVillaId,
                     ApiType = SD.ApiType.POST,
                     Data = request
                 }
                );
               
            }
    }
}
