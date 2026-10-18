namespace RoyalVillaWeb
{
    public static class SD
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE

        }

        public const string SessionAccessToken = "JwtToken";
        public const string SessionRefreshToken = "RefreshToken";
        public const string CurrentApiVersion = "v2";

        public const string NotFoundpage = "~/Views/Shared/NotFound.cshtml";
        public static string ApiBaseURL { get; set; }

        public static string getImageURL(string? Imageurl)
        {
            if (string.IsNullOrEmpty(Imageurl))
            {
                return $"{ApiBaseURL}/images/Placeholder-villa.jpg";
            }
            if (Imageurl.StartsWith("http"))
            {
                return Imageurl;
            }
            return $"{ApiBaseURL}{Imageurl}";
        }
    }
}
