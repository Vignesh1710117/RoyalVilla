using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Options;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using static RoyalVillaWeb.SD;

namespace RoyalVillaWeb.Services
{
    public class BaseService : IBaseService
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenProvider _tokenProvider;

        private const string RefreshingTokenKey = "_RefreshingTokon";
        public ApiResponse<object> ResponseModel { get; set; }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor )
        {
            this.ResponseModel = new();
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsRefeshingToken
        {
            get => (_httpContextAccessor.HttpContext?.Session.GetString(RefreshingTokenKey) == "true");
            set
            {
                if (value)
                {
                    _httpContextAccessor.HttpContext?.Session.SetString(RefreshingTokenKey, "true");
                }
                else
                {
                    _httpContextAccessor.HttpContext?.Session.Remove(RefreshingTokenKey);
                }
            }
        }

        public async Task<T?> SendAsync<T>(ApiRequest apiRequest, bool withBearer =true)
        {
            var vclient = _httpClientFactory.CreateClient("RoyalVillaAPI");

            var message = CreateRequestMessage(apiRequest, withBearer);

            var apiResponse = await vclient.SendAsync(message);

            //401 try with refresh token

            if (apiResponse.StatusCode == HttpStatusCode.Unauthorized && withBearer && !IsRefeshingToken)
            {
                Console.WriteLine("Received 401 Unauthorized - attempting token refresh");
                var refreshed = await RefreshAccesssTokenAsync();
                if (refreshed)
                {
                    // Token refreshed successfully
                    var retryMessage = CreateRequestMessage(apiRequest, withBearer);
                    apiResponse = await vclient.SendAsync(retryMessage);
                }
                else
                {
                    _tokenProvider.ClearToken();
                    await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    _httpContextAccessor.HttpContext?.Response.Redirect("/auth/login");
                    return default;

                }
            }
            if (apiResponse.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            return await apiResponse.Content.ReadFromJsonAsync<T>( );
        }

        private async Task<bool> RefreshAccesssTokenAsync()
        {
            try
            {
                //to counter race condition
                if (IsRefeshingToken)
                {
                    Task.Delay(1000);
                    var accessToken = _tokenProvider.GetAccessToken();

                    if (accessToken != null)
                    {
                        return true;
                    }
                    return false;
                }   

                IsRefeshingToken = true;
                //refreshing here
                var refreshToken = _tokenProvider.GetRefreshToken();
                if(string.IsNullOrEmpty(refreshToken))
                {
                    return false;
                }

                var client = _httpClientFactory.CreateClient("RoyalVillaAPI");
                var refreshRequest = new RefreshTokenRequestDTO 
                {
                    RefreshToken = refreshToken 
                };

                var apiRequest = new ApiRequest
                {
                    ApiType = SD.ApiType.POST,
                    Data = refreshToken,
                    Url = "/api/auth/refresh-token"
                };

                var message = CreateRequestMessage(apiRequest, withBearer: false);

                var apiResponse = await client.SendAsync(message);

                if (apiResponse.IsSuccessStatusCode)
                {
                    var result = await apiResponse.Content.ReadFromJsonAsync<ApiResponse<TokenDTO>>();

                    if(result?.Success == true && result.Data!=null && !string.IsNullOrEmpty(result.Data.AccessToken) &&
        !string.IsNullOrEmpty(result.Data.RefreshToken))
                    {
                        //update token

                        _tokenProvider.SetToken(result.Data.AccessToken, result.Data.RefreshToken);
                        return true;
                    }

                }

                _tokenProvider.ClearToken();
                return false;

            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Token refresh error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                _tokenProvider.ClearToken();
                return false;
            }
            finally
            {
                IsRefeshingToken = false;
            }
        }

        private HttpRequestMessage CreateRequestMessage(ApiRequest apiRequest, bool withBearer)
        {
            

            var message = new HttpRequestMessage
            {
                RequestUri = new Uri(apiRequest.Url, UriKind.Relative),
                Method = GetHttpMethod(apiRequest.ApiType)
            };

            var token = _tokenProvider.GetAccessToken();

            if (withBearer && !string.IsNullOrEmpty(token))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (apiRequest.Data != null)
            {
                if (apiRequest.Data is MultipartContent multipartcontent)
                {
                    message.Content = multipartcontent;
                }
                else
                {
                    message.Content = JsonContent.Create(apiRequest.Data, options: JsonOptions);
                }

            }
            return message;
        }


        private static HttpMethod GetHttpMethod(ApiType apiType)
        {
            return apiType switch
            {
                SD.ApiType.POST => HttpMethod.Post,
                SD.ApiType.PUT => HttpMethod.Put,
                SD.ApiType.DELETE => HttpMethod.Delete,
                _ => HttpMethod.Get
            };
        }
    }
}
