using System;
using System.Collections.Generic;
using System.Text;

namespace RoyalVilla.DTO
{
    public class PaymentVerificationRequestDTO
    {
        //sent from razor
        public string RazorpayOrderId { get; set; } = string.Empty;

        public string RazorpayPaymentId { get; set; } = string.Empty;

        public string RazorpaySignature { get; set; } = string.Empty;
    }
}
