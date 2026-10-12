using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services.IService
{
    public interface IBaseService
    {
        //ApiResponse<object> ResponseModel { get; set; }
        Task<T> SendAsync<T>(ApiRequest apiRequest, bool withBearer);
    }
}
