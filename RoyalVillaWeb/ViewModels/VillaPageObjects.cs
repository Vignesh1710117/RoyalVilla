using RoyalVilla.DTO;

namespace RoyalVillaWeb.ViewModels
{
    public class VillaPageObjects
    {
        public List<VillaDTO> villas { get; set; } = new();
        public VillaQueryParameters villaQuery { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }
}
