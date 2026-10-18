using RoyalVilla_API.Models;

namespace RoyalVillaWeb.Services.IService
{
    public interface IBookingVilla
    {
        Task<T?> BookVilla<T>(int id, BookingVillaDTO bookingVillaDTO);
    }
}
