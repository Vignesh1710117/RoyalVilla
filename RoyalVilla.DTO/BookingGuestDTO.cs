using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RoyalVilla.DTO;
namespace RoyalVilla_API.Models
{
    public class BookingGuestDTO
    {
        public int BookId { get; set; }
        public string firstName { get; set; } = string.Empty;

        public string lastName { get; set; } = string.Empty;

        public int age { get; set; }
    }
}
