using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Controllers
{
    [Route("api/Villa/book/")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IBookVillaService _bookVillaService;
        public BookController(ApplicationDbContext dbContext, IMapper mapper, IBookVillaService bookVillaService)
        {
            _db = dbContext;
            _mapper = mapper;
            _bookVillaService = bookVillaService;
        }

        [HttpPost("{Id:int}")]
        public async Task<ActionResult<ApiResponse<BookVillaDTO>>> BookVilla(int Id, BookingVillaDTO bookingVillaDTO)
        {
            try
            {
                if (Id == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Booking Option for the villaId - {Id} doesnt exist"));
                }

                var villaExist = await _db.Villa.AnyAsync(u => u.Id == Id);

                if (villaExist) {

                    if (bookingVillaDTO == null)
                    {
                        return BadRequest(ApiResponse<object>.BadRequest("Booking details required"));
                    }

                    if (bookingVillaDTO.checkOut.Date <= bookingVillaDTO.checkIn.Date)
                    {
                        return Conflict(ApiResponse<object>.Conflict("CheckOut should be greater than checkIn date"));
                    }

                    var existingBooking = await _db.BookVillas.AnyAsync(u => u.villaId == Id && u.checkIn < bookingVillaDTO.checkOut &&
                                            u.checkOut > bookingVillaDTO.checkIn);
                    if (existingBooking)
                    {
                        return Conflict(
                            ApiResponse<object>.Conflict(
                                "Villa is already booked for the selected dates. Please choose different dates."));
                    }

                    var BookedVilla = _bookVillaService.BookVillaDTOAsync(bookingVillaDTO);
                    BookedVilla.villaId= Id;

                    BookedVilla.BookVillaId = Guid.NewGuid().ToString();


                    BookVilla Bvilla = _mapper.Map<BookVilla>(BookedVilla);

                    if(Bvilla.adult == 0)
                    {
                        return BadRequest(ApiResponse<object>.BadRequest("adult should be atleast one"));
                    }
                    await _db.BookVillas.AddAsync(Bvilla);
                    await _db.SaveChangesAsync();

                    BookVillaDTO responseDTO = _mapper.Map<BookVillaDTO>(Bvilla);

                    var response = ApiResponse<BookVillaDTO>.Ok(responseDTO, "Villa has been Booked");
                    return CreatedAtAction(nameof(BookVilla), new { bookVillaId = BookedVilla.BookVillaId }, response);
                }
                return BadRequest(ApiResponse<object>.BadRequest("Booking Option for the villaId doesnt exist"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while Booking the villa", ex.Message);
                return StatusCode(500, errorResponse);
            }
           
        }

    }
}
