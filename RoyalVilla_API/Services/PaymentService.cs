using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services.IServices;
using System.Security.Cryptography;
using System.Text;

namespace RoyalVilla_API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly ISummaryHelper _summaryHelper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        public PaymentService(ApplicationDbContext dbContext, ISummaryHelper summaryHelper, IConfiguration configuration, ILogger<PaymentService> logger)
        {
            _db = dbContext;
            _summaryHelper = summaryHelper;
            _configuration = configuration;
            _logger = logger;   
        }
        public async Task<PaymentOrderResponseDTO> ProcessPayment(PaymentOrderRequestDTO paymentOrderRequestDTO)
        {
            var Apiresponse = await _db.BookVillas.Include(u => u.villa).FirstOrDefaultAsync(u => u.BookVillaId == paymentOrderRequestDTO.bookVillaId);

            if (Apiresponse == null)
            {
                _logger.LogError($"Booking with Id '{paymentOrderRequestDTO.bookVillaId}' does not exist.");
                throw new InvalidOperationException($"Booking with Id '{paymentOrderRequestDTO.bookVillaId}' does not exist.");
            }

            var existingPayment = await _db.Payments.FirstOrDefaultAsync(p => p.bookVillaDId == Apiresponse.Id && p.PaymentStatus == "Pending");

            var key = _configuration.GetSection("Razorpay")["KeyId"];
            var value = _configuration.GetSection("Razorpay")["KeySecret"];

            if (existingPayment != null)
            {
                _logger.LogInformation($"Existing payment found for booking '{paymentOrderRequestDTO.bookVillaId}'.");
                return new PaymentOrderResponseDTO
                {
                    GatewayOrderId = existingPayment.GatewayOrderId,
                    Amount = existingPayment.Amount,
                    Currency = existingPayment.Currency,
                    RazorpayKeyId = key,

                };
            }

            var totalamount = _summaryHelper.CalculateRate((decimal)Apiresponse.villa.Rate, Apiresponse.NumberOfNights);
            if (totalamount <= 0)
            {
                _logger.LogWarning($"Payment amount for booking '{paymentOrderRequestDTO.bookVillaId}' must be greater than zero.");
                throw new InvalidOperationException($"Payment amount for booking '{paymentOrderRequestDTO.bookVillaId}' must be greater than zero.");
            }
            //

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                _logger.LogError("Razorpay credentials are not configured.");
                throw new InvalidOperationException("Razorpay credentials are not configured.");
            }
            //

            Dictionary<string, object> options = new Dictionary<string, object>
                    {
                        { "amount", checked((long)(totalamount * 100)) },
                        { "currency", "INR" },
                        { "receipt", $"booking_{paymentOrderRequestDTO.bookVillaId}" }
                    };

            RazorpayClient razorpayClient = new RazorpayClient(key, value);

            Order order = razorpayClient.Order.Create(options);

            var response = new PaymentOrderResponseDTO
            {
                GatewayOrderId = order["id"].ToString(),
                Amount = totalamount,
                Currency = "INR",
                RazorpayKeyId = key,

            };

            var payment = new Models.Payment
            {
                bookVillaDId = Apiresponse.Id,
                Amount = response.Amount,
                PaymentStatus = "Pending",
                Currency = response.Currency,
                PaymentGateway = "Razorpay",
                GatewayOrderId = response.GatewayOrderId,
                CreatedAt = DateTime.UtcNow,
                FailureReason = null
            };

            await _db.Payments.AddAsync(payment);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Payment order created successfully for booking '{paymentOrderRequestDTO.bookVillaId}' with Gateway Order ID '{response.GatewayOrderId}'.");
            return response;
        }

       
    }
}
