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

    public class VerificationService : IVerificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<VerificationService> _logger;

        public VerificationService(ApplicationDbContext db, IConfiguration configuration, ILogger<VerificationService> logger)
        {
            _configuration = configuration;
            _db = db;
            _logger = logger;   
        }

        private string GenerateSignature(string orderId, string paymentId, string secret)
        {
            //byte of secret of mine
            byte[] Secretbyte = Encoding.UTF8.GetBytes(secret);

            /*
             
             Or by below was
             byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
             
             */

            var payload = $"{orderId}|{paymentId}";

            //bytes of payload of order and payment id they provide $"___|___";
            byte[] bytesPayload = Encoding.UTF8.GetBytes(payload);

            //Generate the HMAC-SHA256 hash
            using var hmac = new HMACSHA256(Secretbyte);
            var hasBytes = hmac.ComputeHash(bytesPayload);

            //Convert the result to a hexadecimal string
            return Convert.ToHexString(hasBytes).ToLowerInvariant();
        }

        public bool Verification(PaymentVerificationRequestDTO request)
        {
            
           
            string secret = _configuration["Razorpay:KeySecret"]
                ?? throw new InvalidOperationException("Razorpay secret is not configured.");
            
            if(secret == null)
            {
                _logger.LogError("Razorpay secret is not configured.");
            }

            _logger.LogInformation("Starting payment verification process.");
            string generatedSignature = GenerateSignature(request.RazorpayOrderId, request.RazorpayPaymentId, secret);

            byte[] generateSignatureByte;
            byte[] razorpaySignatureByte;

            try
            {
                generateSignatureByte = Convert.FromHexString(generatedSignature);
                razorpaySignatureByte = Convert.FromHexString(request.RazorpaySignature);
            }
            catch (FormatException)
            {
                return false;
            }

            _logger.LogInformation("Payment verification process completed.");
            return generateSignatureByte.Length == razorpaySignatureByte.Length
                &&
                CryptographicOperations.FixedTimeEquals(generateSignatureByte, razorpaySignatureByte);
        }

        public async Task<bool> UpdatePaymentDB(PaymentVerificationRequestDTO request)
        {
            var paymentDBData = await _db.Payments.FirstOrDefaultAsync(u => u.GatewayOrderId == request.RazorpayOrderId);

            if (paymentDBData == null)
            {
                _logger.LogError($"Payment record not found for RazorpayOrderId '{request.RazorpayOrderId}'.");
                throw new InvalidOperationException("Payment record not found.");
            }

            var key = _configuration["Razorpay:KeyId"];
            var secret = _configuration["Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogError("Razorpay credentials are not configured.");
                throw new InvalidOperationException("Razorpay credentials are not configured.");
            }

            if (string.IsNullOrWhiteSpace(request.RazorpayPaymentId))
            {
                _logger.LogError("Payment ID is required for verification.");
                throw new InvalidOperationException("Payment ID is required.");
            }

            RazorpayClient client = new RazorpayClient(key, secret);

            Razorpay.Api.Payment payment = client.Payment.Fetch(request.RazorpayPaymentId);

            if (payment == null)
            {
                _logger.LogError("Unable to retrieve payment details from Razorpay.");
                throw new InvalidOperationException("Unable to retrieve payment details from Razorpay.");
            }

            // Verify that the payment belongs to the expected order.
            string razorpayOrderId = payment["order_id"].ToString()!;

            if (razorpayOrderId != request.RazorpayOrderId)
            {
                _logger.LogError("Payment does not belong to this order.");
                throw new InvalidOperationException("Payment does not belong to this order.");
            }

            // Razorpay returns the amount in paise; the database stores rupees.
            long razorpayAmount = Convert.ToInt64(payment["amount"]);
            long expectedAmountInPaise = checked((long)(paymentDBData.Amount * 100m));

            if (razorpayAmount != expectedAmountInPaise)
            {
                _logger.LogError($"Payment amount mismatch: expected {expectedAmountInPaise} paise, got {razorpayAmount} paise.");
                throw new InvalidOperationException($"Payment amount mismatch: expected {expectedAmountInPaise} paise, got {razorpayAmount} paise.");
            }

            string currency = payment["currency"].ToString()!;

            if (currency != paymentDBData.Currency)
            {
                _logger.LogError($"Payment currency mismatch: expected {paymentDBData.Currency}, got {currency}.");
                throw new InvalidOperationException($"Payment currency mismatch: expected {paymentDBData.Currency}, got {currency}.");
            }

            string status = payment["status"].ToString()!;

            if (status != "captured")
            {
                _logger.LogError($"Payment has not been captured. Current status: {status}.");
                throw new InvalidOperationException("Payment has not been captured.");
            }

            paymentDBData.GatewayPaymentId = request.RazorpayPaymentId;
            paymentDBData.PaymentStatus = "Success";
            paymentDBData.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            _logger.LogInformation($"Payment record updated successfully for RazorpayOrderId '{request.RazorpayOrderId}' with Payment ID '{request.RazorpayPaymentId}'.");
            return true;
        }


    }
}
