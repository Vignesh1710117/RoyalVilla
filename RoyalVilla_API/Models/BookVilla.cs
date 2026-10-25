using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalVilla_API.Models
{
    public class BookVilla
    {
        public int Id { get; set; }

        public string BookVillaId { get; set; }
        public DateTime checkIn { get; set; }
        public DateTime checkOut { get; set; }
        public int adult { get; set; }
        public int children { get; set; }
        public int NumberOfNights { get; set; }

        public ICollection<Guests> Guests { get; set; } = new List<Guests>();

        [Required]
        [ForeignKey(nameof(Villa))]
        public int villaId { get; set; }
        public Villa? villa { get; set; }

    }
}
