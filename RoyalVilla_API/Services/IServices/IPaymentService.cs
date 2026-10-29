using RoyalVilla.DTO;

namespace RoyalVilla_API.Services.IServices
{
    public interface IPaymentService
    {
        Task<PaymentOrderResponseDTO> ProcessPayment(PaymentOrderRequestDTO paymentOrderRequestDTO);
        
    }
}
