using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalVilla_API.Models
{
    public class BookingVillaDTO
    {
        public DateTime checkIn { get; set; }
        public DateTime checkOut { get; set; }
        public int adult { get; set; }
        public int children { get; set; }


    }
}
