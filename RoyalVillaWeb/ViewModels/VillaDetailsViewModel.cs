using RoyalVilla.DTO;

namespace RoyalVillaWeb.ViewModels
{
    public class VillaDetailsViewModel
    {
        public VillaDTO Villa { get; set; } = default!;

        public List<VillaAmentiesDTO> Amenities { get; set; } = new();
    }
}
