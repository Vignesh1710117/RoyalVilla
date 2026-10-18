using RoyalVilla_API.Models;

namespace RoyalVilla_API.Services.IServices
{
    public interface IBookVillaService
    {
        BookVillaDTO BookVillaDTOAsync(BookingVillaDTO bookingVillaDTO);

    }
}
