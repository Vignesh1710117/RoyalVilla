using AutoMapper.Configuration.Conventions;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Models;
using RoyalVillaWeb.Services;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Controllers
{
    public class GuestController : Controller
    {
        private readonly IGuest _guest;
        public GuestController(IGuest guest)
        {
            _guest = guest;
        }
        public async Task<IActionResult> Index(string bookVillaId, int adult , int children)
        {
            var totalCount = adult + children;
            var model = new AddBookingGuestsDTO
            {
                BookVillaId = bookVillaId,
                Guests = Enumerable.Range(0, totalCount).Select(u => new BookingGuestDTO()).ToList()

            };

            return View(model);
        }
        public async Task<IActionResult> GuestSubmission(string bookVillaId,AddBookingGuestsDTO request)
        {
            if (!ModelState.IsValid)
            {
                return await ReloadGuestPage(bookVillaId, request);
            }

            var response = await _guest.AddingGuest<ApiResponse<AddBookingGuestsDTO>>(bookVillaId, request);

            if (response != null && response.Success && response.Data != null)
            {
                return RedirectToAction(
                    "Index",
                    "Summary",
            new { bookVillaId });
            }

            return await ReloadGuestPage(bookVillaId, request);
        }

        private Task<IActionResult> ReloadGuestPage(string bookVillaId, AddBookingGuestsDTO request)
        {
            request.BookVillaId = bookVillaId;

            return Task.FromResult<IActionResult>(View("Index", request));
        }
    }
}
