using AutoMapper;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Services
{
    public class BookVillaService : IBookVillaService
    {
        private readonly IMapper _mapper;
        public BookVillaService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public BookVillaDTO BookVillaDTOAsync(BookingVillaDTO bookingVillaDTO)
        {
            if (bookingVillaDTO.checkOut.Date <= bookingVillaDTO.checkIn.Date)
            {
                throw new ArgumentException(
                    "Check-out must be after check-in.");
            }
            var dayCount = DayCalculator(bookingVillaDTO.checkIn, bookingVillaDTO.checkOut);

            var booking = _mapper.Map<BookVillaDTO>(bookingVillaDTO);
            booking.NumberOfNights = dayCount;

            return booking;
        }
        private int DayCalculator(DateTime checkin, DateTime checkout)
        {
            TimeSpan duration = checkout - checkin;

            int noOfDays = duration.Days;

            return noOfDays;
        }
    }
}
