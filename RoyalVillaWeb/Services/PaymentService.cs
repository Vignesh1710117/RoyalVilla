using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Services
{
    public class PaymentService : BaseService, IPaymentService
    {
        public PaymentService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor) 
            : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {
            
        }
        public Task<T?> PaymentApi<T>(PaymentOrderRequestDTO OrderRequest)
        {
            return SendAsync<T>(
                 new ApiRequest 
                 { 
                     ApiType = SD.ApiType.POST,
                     Data = OrderRequest,
                     Url = $"api/payment/OrderRequest"
                 }
                );

        }

        public Task<T?> VerifyApi<T>(PaymentVerificationRequestDTO paymentVerificationRequest)
        {
            return SendAsync<T>(
                 new ApiRequest
                 {
                     ApiType = SD.ApiType.POST,
                     Data = paymentVerificationRequest,
                     Url = "api/payment/verify"
                 }
                );
        }
    }
}
