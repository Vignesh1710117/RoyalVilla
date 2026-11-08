using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;
using RoyalVillaWeb.ViewModels;
using System.Diagnostics;

namespace RoyalVillaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IMapper _mapper;
        public HomeController(IVillaService villaService, IMapper mapper)
        {
            _villaService = villaService;
            _mapper = mapper;
        }

        //[Authorize]
        public async Task<IActionResult> Index(VillaQueryParameters parameters)
        {

            try
            {
                var response = await _villaService.GetAllAsync<ApiResponse<List<VillaDTO>>>(parameters);
                if (response != null && response.Success && response.Data != null)
                {
                    int Header(string name) =>
    int.TryParse(Response.Headers[name].FirstOrDefault(), out var v) ? v : 0;
                    var villas = new VillaPageObjects
                    {
                        villas = response.Data,
                        villaQuery = parameters,
                        Pagination = response.Pagination
                    };
                    return View(villas);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occured {ex.Message} ";
            }


            return View(new VillaPageObjects());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
