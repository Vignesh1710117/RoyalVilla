namespace RoyalVillaWeb.Services.IService
{
    public interface ISummary
    {
        Task<T?> GetSummaryAsync<T>(string bookVillaId);
    }
}
