using RoyalVilla_API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RoyalVilla.DTO
{
    public class SummaryDTO
    {
        public int VillaId { get; set; }
        public string Name { get; set; }
        public string? Details { get; set; }
        public int Sqft { get; set; }
        public int Occupancy { get; set; }
        public string? ImageUrl { get; set; }
        public List<VillaAmentiesDTO> AmenitiesName { get; set; } = new();
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal NumberOfNights { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalAmount { get; set; }
        public string BookVillaId { get; set; }
        public List<BookingGuestDTO> Guests { get; set; } = new();
    }
}