using System;
using System.Collections.Generic;
using System.Text;

namespace RoyalVilla.DTO
{
    public class PaymentOrderResponseDTO
    {
        public int PaymentId { get; set; }

        public string GatewayOrderId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "INR";

        public string RazorpayKeyId { get; set; } = string.Empty;
    }
}
