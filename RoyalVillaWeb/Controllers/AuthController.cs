using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IService;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoyalVillaWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IMapper _mapper;
        public AuthController(IAuthService authService, IMapper mapper, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _mapper = mapper;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDTO login)
        {

            try
            {
                var response = await _authService.LoginAsync<ApiResponse<TokenDTO>>(login);
                if (response != null && response.Success && response.Data != null)
                {
                    var principle = _tokenProvider.CreatePrincipleFromJwtToken(response.Data.AccessToken);
                    if (principle != null) 
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principle);
                        _tokenProvider.SetToken(response.Data.AccessToken, response.Data.RefreshToken);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["error"] = "Invalid Token Received. Please try again";
                    }


                }
                else
                {
                    TempData["error"] = response.Message;
                }
                return View(login);

            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occured {ex.Message} ";
            }

            return View();
        }

        [HttpGet]
        public IActionResult register()
        {
            return View(
               new RegisterationRequestDTO
               {
                   Email = string.Empty,
                   Name = string.Empty,
                   Password = string.Empty,
                   Role = "Customer"
               });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterationRequestDTO registerationRequestDTO)
        {

            try
            {
                ApiResponse<UserDTO> response = await _authService.RegisterAsync<ApiResponse<UserDTO>>(registerationRequestDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Registeration is successful! Please login with credential";
                    return RedirectToAction("Login");
                }

                else
                {
                    TempData["error"] = response?.Message ?? "Registeration is failed, Please try again";
                    return View(registerationRequestDTO);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occured {ex.Message} ";
            }

            return View(registerationRequestDTO);
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }
    }
}

//TokenDTO modal = response.Data;

//var handler = new JwtSecurityTokenHandler();
//var jwt = handler.ReadJwtToken(modal.AccessToken);

////foreach (var claim in jwt.Claims)
////{
////    Console.WriteLine($"{claim.Type} : {claim.Value}");
////}



//var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
//identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(u => u.Type == "email").Value));
//identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));

//var principle = new ClaimsPrincipal(identity);

//await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principle);
//HttpContext.Session.SetString(SD.SessionToken, modal.AccessToken);
//return RedirectToAction("Index", "Home");

