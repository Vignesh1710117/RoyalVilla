using RoyalVilla.DTO;

namespace RoyalVillaWeb.Services.IService
{
    public interface IVillaAmenitiesService
    {
        Task<T?> GetAmenityAsync<T>(int id);
    }
}