using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;

namespace RoyalVilla_API.Controllers
{
    [Route("api/guests/")]
    [ApiController]
    public class BookingGuestController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    private readonly IMapper _mapper;
    private readonly ILogger<BookingGuestController> _logger;
    public BookingGuestController(ApplicationDbContext db, IMapper mapper, ILogger<BookingGuestController> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost("{bookVillaId}")]
    public async Task<ActionResult<AddBookingGuestsDTO>> AddGuest(string bookVillaId, AddBookingGuestsDTO request)
    {
        try
        {
            if (request.Guests == null || !request.Guests.Any())
            {
                _logger.LogError("At least one guest is required.");
                    return BadRequest(ApiResponse<object>.BadRequest("At least one guest is required."));
            }
            var BookingExist = _db.BookVillas.FirstOrDefault(u => u.BookVillaId == request.BookVillaId);

            if (BookingExist == null)
            {
                    _logger.LogError($"Booking with Id '{request.BookVillaId}' does not exist");
                    return NotFound(ApiResponse<object>.NotFound($"Booking with Id '{request.BookVillaId}' does not exist"));
            }
            var guests = request.Guests.Select(g => new Guests
            {
                BookId = BookingExist.Id,
                firstName = g.firstName,
                lastName = g.lastName,
                age = g.age,

            }).ToList();

            await _db.Guests.AddRangeAsync(guests);
            await _db.SaveChangesAsync();

                var response = new AddBookingGuestsDTO
                {
                    BookVillaId = bookVillaId,
                    Guests = _mapper.Map<List<BookingGuestDTO>>(guests)
                };
                _logger.LogInformation("Guest details added successfully", response.BookVillaId, response.Guests);
                return Ok(ApiResponse<AddBookingGuestsDTO>.Ok(response, "Details added succesfully"));

        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while adding guest details: {ex.Message}");
            return BadRequest(ApiResponse<object>.BadRequest($"An error occurred while adding guest details: {ex.Message}"));
        }
    }
}
}