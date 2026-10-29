using RoyalVilla.DTO;

namespace RoyalVilla_API.Services.IServices
{
    public interface IVerificationService
    {
        bool Verification(PaymentVerificationRequestDTO request);
        Task<bool> UpdatePaymentDB(PaymentVerificationRequestDTO request);
    }
}
