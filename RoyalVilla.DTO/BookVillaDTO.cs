using RoyalVilla.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalVilla_API.Models
{
    public class BookVillaDTO
    {
        public string BookVillaId { get; set; }

        public int villaId { get; set; }

        [Required]
        public DateTime checkIn { get; set; }

        [Required]
        public DateTime checkOut { get; set; }

        [Required]
        public int adult { get; set; }

        [Required]
        public int children { get; set; }
        public int NumberOfNights { get; set; }

        public VillaDTO? Villa { get; set; }
    }
}
