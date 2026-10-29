using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Models;
using RoyalVillaWeb.Services;
using RoyalVillaWeb.Services.IService;
using RoyalVillaWeb.ViewModels;

namespace RoyalVillaWeb.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index(string bookVillaId)
        {
            PaymentOrderRequestDTO request = new PaymentOrderRequestDTO
            {
                bookVillaId = bookVillaId
            };//we can directly pass bookVillaId just for learning did this

            var Paymentresponse =await _paymentService.PaymentApi<ApiResponse<PaymentOrderResponseDTO>>(request);

            if (Paymentresponse?.Data == null)
            {
                return NotFound();
            }

            PaymentApiViewModel model = new PaymentApiViewModel
            {
                amount = Paymentresponse.Data.Amount,
                Currency = Paymentresponse.Data.Currency,
                keyId = _configuration["Razorpay:KeyId"],
                OrderId = Paymentresponse.Data.GatewayOrderId
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Verify([FromBody] PaymentVerificationRequestDTO request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.RazorpayOrderId) ||
                string.IsNullOrWhiteSpace(request.RazorpayPaymentId) ||
                string.IsNullOrWhiteSpace(request.RazorpaySignature))
            {
                return BadRequest("Payment verification details are required.");
            }

            var response = await _paymentService.VerifyApi<ApiResponse<bool>>(request);

            if (response?.Data != true)
            {
                return BadRequest("Payment verification failed.");
            }

            return Ok(new { success = true, message = "Payment successful." });
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
