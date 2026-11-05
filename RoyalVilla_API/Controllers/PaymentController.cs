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
        private readonly IPaymentService _paymentService;
        private readonly IVerificationService _verificationService;
        private readonly ILogger<PaymentController> _logger;
        public PaymentController(IPaymentService paymentService, IVerificationService verificationService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _verificationService = verificationService;
            _logger = logger;
        }

        [HttpPost("OrderRequest")]
        public async Task<ActionResult<PaymentOrderResponseDTO>> Payment(PaymentOrderRequestDTO OrderRequest)
        {
            if (OrderRequest == null || string.IsNullOrEmpty(OrderRequest.bookVillaId))
            {
                _logger.LogError("BookVillaId is required for payment processing.");
                return BadRequest(ApiResponse<object>.BadRequest("BookVillaId is required for payment processing."));
            }

            var response = await _paymentService.ProcessPayment(OrderRequest);

            if (response == null)
            {
                _logger.LogError($"Payment processing failed for BookVillaId '{OrderRequest.bookVillaId}'.");
                return NotFound(ApiResponse<object>.NotFound($"Payment processing failed for BookVillaId '{OrderRequest.bookVillaId}'."));
            }

            return Ok(ApiResponse<PaymentOrderResponseDTO>.Ok(response, "Payment sent to client"));
        }


        [HttpPost("verify")]
        public async Task<ActionResult<bool>> verify(PaymentVerificationRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RazorpayOrderId) ||string.IsNullOrWhiteSpace(request.RazorpayPaymentId) ||
                 string.IsNullOrWhiteSpace(request.RazorpaySignature))
            {
                _logger.LogError("Payment verification details are required.");
                return BadRequest(ApiResponse<object>.BadRequest("Payment verification details are required."));
            }

            var isVerified = _verificationService.Verification(request);

            if (!isVerified)
            {
                _logger.LogError("Invalid payment signature.");
                return BadRequest(ApiResponse<object>.BadRequest("Invalid payment signature."));
            }
            
            var PaymentDbupdate = await _verificationService.UpdatePaymentDB(request);

            _logger.LogInformation("Payment verified and database updated successfully for RazorpayOrderId '{RazorpayOrderId}'.", request.RazorpayOrderId);
            return Ok(ApiResponse<bool>.Ok(PaymentDbupdate, "Signature verified successfully and payment DB updated"));
        }
    }
}
