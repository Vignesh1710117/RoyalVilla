using RoyalVilla.DTO;
using RoyalVillaWeb.ViewModels;

namespace RoyalVillaWeb.Services.IService
{
    public interface IVillaService
    {
        Task<T?> GetAllAsync<T>(VillaQueryParameters parameters);
        Task<T?> GetAsync<T>(int id);
        Task<T?> CreateAsync<T>(VillaCreateDTO dto);
        Task<T?> UpdateAsync<T>(VillaUpdateDTO dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
