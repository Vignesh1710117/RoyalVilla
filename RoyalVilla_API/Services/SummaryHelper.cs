using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Services
{
    public class SummaryHelper : ISummaryHelper
    {
        public decimal CalculateRate(decimal rate, decimal numberOfNights)
        {
            return rate * numberOfNights;
        }
    }
}
