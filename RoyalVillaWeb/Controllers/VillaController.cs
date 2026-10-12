    using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;
using System.Diagnostics;
using System.Security.Claims;

namespace RoyalVillaWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IMapper _mapper;
        public VillaController(IVillaService villaService, IMapper mapper)
        {
            _villaService = villaService;
            _mapper = mapper;
        }
        // for check i wrote ignore
        [Authorize]
        public IActionResult CheckRole()
        {
            return Content(
                $"Authenticated: {User.Identity?.IsAuthenticated}, " +
                $"Is Admin: {User.IsInRole("Admin")}, " +
                $"Roles: {string.Join(", ", User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value))}"
            );
        }

        public async Task<IActionResult> Index()
        {
            List<VillaDTO> villaList = new();

            try
            {
                var response = await _villaService.GetAllAsync<ApiResponse<List<VillaDTO>>>();
                if (response!=null && response.Success && response.Data !=null)
                {
                    villaList = response.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occured {ex.Message} ";
            }

            return View(villaList);
        }

        public async Task<IActionResult> Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VillaCreateDTO createDTO)
        {
            if(!ModelState.IsValid)
            {
                return View(createDTO);

            }

            try
            {
                var response = await _villaService.CreateAsync<ApiResponse<VillaCreateDTO>>(createDTO);

                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = $"Villa is created successfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occured {ex.Message} ";
            }

            return View(createDTO);


        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "Invalid villa ID";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var response = await _villaService.GetAsync<ApiResponse<VillaDTO>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(VillaDTO villa)
        {

            try
            {
                var response = await _villaService.DeleteAsync<ApiResponse<object>>(villa.Id);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa is deleted successfully";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }


            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "Invalid villa ID";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var response = await _villaService.GetAsync<ApiResponse<VillaUpdateDTO>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }


            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Edit(VillaUpdateDTO villa)
        {

            try
            {
                var response = await _villaService.UpdateAsync<ApiResponse<VillaUpdateDTO>>(villa);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = $"Villa is retrived successfully";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }


            return RedirectToAction(nameof(Index));
        }



    }
}
