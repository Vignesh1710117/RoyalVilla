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
        public PaymentService(ApplicationDbContext dbContext, ISummaryHelper summaryHelper, IConfiguration configuration)
        {
            _db = dbContext;
            _summaryHelper = summaryHelper;
            _configuration = configuration;
        }
        public async Task<PaymentOrderResponseDTO> ProcessPayment(PaymentOrderRequestDTO paymentOrderRequestDTO)
        {
            var Apiresponse = await _db.BookVillas.Include(u => u.villa).FirstOrDefaultAsync(u => u.BookVillaId == paymentOrderRequestDTO.bookVillaId);

            if (Apiresponse == null)
            {
                throw new InvalidOperationException("Booking not found.");
            }

            var existingPayment = await _db.Payments.FirstOrDefaultAsync(p => p.bookVillaDId == Apiresponse.Id && p.PaymentStatus == "Pending");

            var key = _configuration.GetSection("Razorpay")["KeyId"];
            var value = _configuration.GetSection("Razorpay")["KeySecret"];

            if (existingPayment != null)
            {
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
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            }
            //

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    "Razorpay credentials are not configured.");
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

            return response;
        }

       
    }
}
