using RoyalVilla.DTO;

namespace RoyalVillaWeb.Services.IService
{
    public interface IPaymentService
    {
        Task<T?> PaymentApi<T>(PaymentOrderRequestDTO paymentOrderRequest);
        Task<T?> VerifyApi<T>(PaymentVerificationRequestDTO paymentVerificationRequest);

    }
}
