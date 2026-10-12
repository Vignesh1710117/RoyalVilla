using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Controllers
{
    public class VillaAmenitiesController : Controller
    {
        private readonly IVillaAmenitiesService _VillaAmenitiesService;
        private readonly IMapper _mapper;
        public VillaAmenitiesController(IMapper mapper, IVillaAmenitiesService villaAmenitiesService)
        {
            _VillaAmenitiesService = villaAmenitiesService;
            _mapper = mapper;
        }

        // GET: VillaAmenitiesController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            List<VillaAmentiesDTO> villaAmentiesDTOs = null;
            try
            {
                var response = await _VillaAmenitiesService.GetAmenityAsync<ApiResponse<List<VillaAmentiesDTO>>>(id);

                if (response != null)
                {
                    villaAmentiesDTOs = response.Data;
                    return View(villaAmentiesDTOs);
                }
            }
            catch (Exception ex)
            {
                {
                    TempData["error"] = $"Error occured will retriving the Amenitied {ex.Message}";
                }

            }

            return View();
        }
    }
}
