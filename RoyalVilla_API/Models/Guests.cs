using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalVilla_API.Models
{
    public class Guests
    {
        public int Id { get; set; }
        public string firstName { get; set; } = string.Empty;

        public string lastName { get; set; } = string.Empty;

        public int age { get; set; }

        [Required]  
        [ForeignKey(nameof(Booking))]
        public int BookId { get; set; }
        public BookVilla Booking { get; set; }
    }
}
