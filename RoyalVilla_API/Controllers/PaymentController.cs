using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Controllers
{
    [Route("api/payment")]
    [ApiController]


    public class PaymentController : ControllerBase
    {
        public readonly IPaymentService _paymentService;
        public readonly IVerificationService _verificationService;
        public PaymentController(IPaymentService paymentService, IVerificationService verificationService)
        {
            _paymentService = paymentService;
            _verificationService = verificationService;
        }

        [HttpPost("OrderRequest")]
        public async Task<ActionResult<PaymentOrderResponseDTO>> Payment(PaymentOrderRequestDTO OrderRequest)
        {
            if (OrderRequest == null || string.IsNullOrEmpty(OrderRequest.bookVillaId))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Booking Id not found"));
            }

            var response = await _paymentService.ProcessPayment(OrderRequest);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.NotFound("Book not found"));
            }

            return Ok(ApiResponse<PaymentOrderResponseDTO>.Ok(response, "Payment sent to client"));
        }


        [HttpPost("verify")]
        public async Task<ActionResult<bool>> verify(PaymentVerificationRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RazorpayOrderId) ||string.IsNullOrWhiteSpace(request.RazorpayPaymentId) ||
                 string.IsNullOrWhiteSpace(request.RazorpaySignature))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Payment verification details are required."));
            }

            var isVerified = _verificationService.Verification(request);

            if (!isVerified)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Invalid payment signature."));
            }
            
            var PaymentDbupdate = await _verificationService.UpdatePaymentDB(request);

            return Ok(ApiResponse<bool>.Ok(PaymentDbupdate, "Signature verified successfully and payment DB updated"));
        }
    }
}
