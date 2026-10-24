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
    public BookingGuestController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpPost("{bookVillaId}")]
    public async Task<ActionResult<AddBookingGuestsDTO>> AddGuest(string bookVillaId, AddBookingGuestsDTO request)
    {
        try
        {
            if (request.Guests == null || !request.Guests.Any())
            {

                return BadRequest(ApiResponse<object>.BadRequest("At least one guest is required."));
            }
            var BookingExist = _db.BookVillas.FirstOrDefault(u => u.BookVillaId == request.BookVillaId);

            if (BookingExist == null)
            {
                return NotFound(ApiResponse<object>.NotFound("Booking not found."));
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

                return Ok(ApiResponse<AddBookingGuestsDTO>.Ok(response, "Details added succesfully"));

        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.BadRequest($"An error occured while entering the details {ex}"));
        }
    }
}
}