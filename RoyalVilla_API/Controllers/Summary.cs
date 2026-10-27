using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services.IServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RoyalVilla_API.Controllers
{
    [Route("api/summary")]
    [ApiController]
    public class Summary : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ISummaryHelper _helper;
        public Summary(ApplicationDbContext db,IMapper mapper, ISummaryHelper helper)
        {
            _db = db;
            _mapper = mapper;
            _helper = helper;
        }

        [HttpGet("{bookVillaId}")]
        public async Task<ActionResult<SummaryDTO>> GetSummaryDetails(string bookVillaId)
        {
            if (bookVillaId == null)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Booking details required"));
            }
            //var booking = _db.BookVillas.FirstOrDefault(b => b.BookVillaId == bookVillaId);
            var Apiresponse = await _db.BookVillas.Include(u => u.villa).ThenInclude(u => u.Amenities).Include(u => u.Guests).FirstOrDefaultAsync(u => u.BookVillaId == bookVillaId);

            if (Apiresponse == null)
            {  
                return NotFound(ApiResponse<object>.NotFound("Booking details Not found")); 
            }
            var totalamount = _helper.CalculateRate((decimal)Apiresponse.villa.Rate, Apiresponse.NumberOfNights);

            SummaryDTO summaryResponse = new SummaryDTO
            {
                BookVillaId = bookVillaId,
                VillaId = Apiresponse.villa.Id,
                Name = Apiresponse.villa.Name,
                Details = Apiresponse.villa.Details,
                Sqft = Apiresponse.villa.Sqft,
                Occupancy = Apiresponse.villa.Occupancy,
                ImageUrl = Apiresponse.villa.ImageUrl,
                AmenitiesName = Apiresponse.villa.Amenities.Select(u => new VillaAmentiesDTO
                {   
                    Name = u.Name,
                    Description = u.Description
                }).ToList(),
                CheckIn = Apiresponse.checkIn,
                CheckOut = Apiresponse.checkOut,
                NumberOfNights = Apiresponse.NumberOfNights,
                PricePerNight = (decimal)Apiresponse.villa.Rate,
                TotalAmount = totalamount,
                Guests = Apiresponse.Guests.Select(u => new BookingGuestDTO
                {
                   firstName =  u.firstName,
                   lastName = u.lastName,
                   age = u.age
                }).ToList(),
            };

            //var response = _mapper.Map<SummaryDTO>(Apiresponse);
            //response.TotalAmount = Totalamount;

            return Ok(ApiResponse<SummaryDTO>.Ok(summaryResponse, "Retrived summary details"));
        }
    }
}
