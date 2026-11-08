namespace RoyalVillaWeb.ViewModels
{
    public class VillaQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 3;
        public string? FilterBy { get; set; }
        public string? FilterQuery { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
}
