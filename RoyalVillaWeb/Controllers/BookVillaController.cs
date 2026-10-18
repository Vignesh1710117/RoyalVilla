using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Models;
using RoyalVillaWeb.Services;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Controllers
{
    public class BookVillaController : Controller
    {
        private readonly IBookingVilla _bookingVilla;
        private readonly IVillaService _villaService;
        public BookVillaController(IBookingVilla bookingVilla, IVillaService villaService)
        {
            _bookingVilla = bookingVilla;
            _villaService = villaService;
        }

        public async Task<IActionResult> BookingPage(int id)
        {
            var villaDetails = await _villaService.GetAsync<ApiResponse<VillaDTO>>(id);

            if (villaDetails?.Data == null)
            {
                return NotFound();
            }

            BookVillaDTO model = new BookVillaDTO
            {
                Villa = villaDetails.Data
            };
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitBooking(int id, BookingVillaDTO bookingVillaDTO)
        {
            if(!ModelState.IsValid)
            {
                return await ReloadBookingPage(id, bookingVillaDTO);
            }

            try
            {
                var response = await _bookingVilla.BookVilla<ApiResponse<BookVillaDTO>>(id, bookingVillaDTO);
  

                if (response != null && response.Success && response.Data != null)
                {
                    var villaName = await _villaService.GetAsync<ApiResponse<VillaDTO>>(id);
                    TempData["success"] =
                      $"Booking confirmed! {villaName?.Data?.Name} from " +
                    $"{bookingVillaDTO.checkIn:dd MMM yyyy} to " +
                    $"{bookingVillaDTO.checkOut:dd MMM yyyy}.";

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Unable to submit your booking.");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occured {ex.Message} ";
            }
            return await ReloadBookingPage(id, bookingVillaDTO);
        }

        private async Task<IActionResult> ReloadBookingPage(int id, BookingVillaDTO bookingVillaDTO)
        {
            var villaResponse =
                await _villaService.GetAsync<ApiResponse<VillaDTO>>(id);

            if (villaResponse?.Data == null)
            {
                return NotFound();
            }

            var model = new BookVillaDTO
            {
                villaId = id,
                Villa = villaResponse.Data,
                checkIn = bookingVillaDTO.checkIn,
                checkOut = bookingVillaDTO.checkOut,
                adult = bookingVillaDTO.adult,
                children = bookingVillaDTO.children
            };

            return View("BookingPage", model);
        }
    }
}
