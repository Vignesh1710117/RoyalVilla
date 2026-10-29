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
        public VerificationService(ApplicationDbContext db, IConfiguration configuration)
        {
            _configuration = configuration;
            _db = db;
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

            return generateSignatureByte.Length == razorpaySignatureByte.Length
                &&
                CryptographicOperations.FixedTimeEquals(generateSignatureByte, razorpaySignatureByte);
        }

        public async Task<bool> UpdatePaymentDB(PaymentVerificationRequestDTO request)
        {
            var paymentDBData = await _db.Payments.FirstOrDefaultAsync(u => u.GatewayOrderId == request.RazorpayOrderId);

            if (paymentDBData == null)
            {
                throw new InvalidOperationException("Payment record not found.");
            }

            var key = _configuration["Razorpay:KeyId"];
            var secret = _configuration["Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException("Razorpay credentials are not configured.");
            }

            if (string.IsNullOrWhiteSpace(request.RazorpayPaymentId))
            {
                throw new InvalidOperationException("Payment ID is required.");
            }

            RazorpayClient client = new RazorpayClient(key, secret);

            Razorpay.Api.Payment payment = client.Payment.Fetch(request.RazorpayPaymentId);

            if (payment == null)
            {
                throw new InvalidOperationException("Unable to retrieve payment details from Razorpay.");
            }

            // Verify that the payment belongs to the expected order.
            string razorpayOrderId = payment["order_id"].ToString()!;

            if (razorpayOrderId != request.RazorpayOrderId)
            {
                throw new InvalidOperationException("Payment does not belong to this order.");
            }

            // Razorpay returns the amount in paise; the database stores rupees.
            long razorpayAmount = Convert.ToInt64(payment["amount"]);
            long expectedAmountInPaise = checked((long)(paymentDBData.Amount * 100m));

            if (razorpayAmount != expectedAmountInPaise)
            {
                throw new InvalidOperationException("Payment amount does not match.");
            }

            string currency = payment["currency"].ToString()!;

            if (currency != paymentDBData.Currency)
            {
                throw new InvalidOperationException("Payment currency does not match.");
            }

            string status = payment["status"].ToString()!;

            if (status != "captured")
            {
                throw new InvalidOperationException("Payment has not been captured.");
            }

            paymentDBData.GatewayPaymentId = request.RazorpayPaymentId;
            paymentDBData.PaymentStatus = "Success";
            paymentDBData.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return true;
        }


    }
}
