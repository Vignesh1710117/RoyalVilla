using RoyalVilla_API.Models;
using RoyalVilla.DTO;
namespace RoyalVillaWeb.Services.IService
{
    public interface IGuest
    {
        Task<T?> AddingGuest<T>(string BookVillaId, AddBookingGuestsDTO request);
    }
}
