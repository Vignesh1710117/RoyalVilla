using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Services;
using RoyalVillaWeb.Services.IService;
using RoyalVillaWeb.ViewModels;

namespace RoyalVillaWeb.Controllers
{
    public class VillaAmenitiesController : Controller
    {
        private readonly IVillaAmenitiesService _VillaAmenitiesService;
        private readonly IVillaService _villaService;
        private readonly IMapper _mapper;
        public VillaAmenitiesController(IMapper mapper, IVillaAmenitiesService villaAmenitiesService, IVillaService villaService)
        {
            _VillaAmenitiesService = villaAmenitiesService;
            _mapper = mapper;
            _villaService = villaService;
        }

        // GET: Villa-amenities/villa/
        public async Task<IActionResult> Details(int id)
        {
            //List<VillaAmentiesDTO> villaAmenties = new ();
            try
            {
                var villaResponse = await _villaService.GetAsync<ApiResponse<VillaDTO>>(id);

                if (villaResponse?.Data == null)
                {
                    return View(SD.NotFoundpage);
                }

                var amenitiesResponse = await _VillaAmenitiesService.GetAmenityAsync<ApiResponse<List<VillaAmentiesDTO>>>(id);

                var model = new VillaDetailsViewModel
                {
                    Villa = villaResponse.Data,
                    Amenities = amenitiesResponse?.Data ?? new List<VillaAmentiesDTO>()
                };

                return View(model);
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
