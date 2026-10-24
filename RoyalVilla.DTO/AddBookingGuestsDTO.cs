using RoyalVilla.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalVilla_API.Models
{
    public class AddBookingGuestsDTO
    {
        public string BookVillaId { get; set; }

        public List<BookingGuestDTO> Guests { get; set; }
    }
}
