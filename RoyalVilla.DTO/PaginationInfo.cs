namespace RoyalVilla.DTO
{
    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public interface IPagedResponse
    {
        PaginationInfo? Pagination { get; set; }
    }
}
