using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Services
{
    public class SummaryService : BaseService, ISummary
    {
        public SummaryService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
    : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {
        }

        private const string APIEndpoint = "api/summary/";
        public Task<T?> GetSummaryAsync<T>(string bookVillaId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = APIEndpoint + bookVillaId,
            });
        }
    }
}
