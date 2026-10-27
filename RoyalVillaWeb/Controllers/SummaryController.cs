using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Models;
using RoyalVillaWeb.Services.IService;

namespace RoyalVillaWeb.Controllers
{
    public class SummaryController : Controller
    {
        private readonly ISummary _summary;
        public SummaryController(ISummary summary)
        {
            _summary = summary;
        }
        public async Task<IActionResult> Index(string bookVillaId)
        {
            var response = await _summary.GetSummaryAsync<ApiResponse<SummaryDTO>>(bookVillaId);

            if (response?.Data == null)
            {
                return NotFound();
            }

            return View(response.Data);
        }
    }
}
