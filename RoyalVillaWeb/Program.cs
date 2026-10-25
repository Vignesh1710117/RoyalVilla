using Microsoft.AspNetCore.Authentication.Cookies;
using RoyalVilla.DTO;
using RoyalVillaWeb;
using RoyalVillaWeb.Services;
using RoyalVillaWeb.Services.IService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
    
    builder.Services.AddAutoMapper(o =>
    {
        o.CreateMap<VillaDTO, VillaCreateDTO>().ReverseMap();
        o.CreateMap<VillaDTO, VillaUpdateDTO>().ReverseMap();
    });
    
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
    {
        option.Cookie.HttpOnly = true;
        option.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        option.SlidingExpiration = true;
        option.LoginPath = "/auth/login"; //to override url after authorise , if not it will route to account/login
        option.AccessDeniedPath = "/auth/accessdenied";
    });

//

//

//

//

    SD.ApiBaseURL = builder.Configuration.GetValue<string>("ServiceUrls:VillaAPI");
    builder.Services.AddHttpClient("RoyalVillaAPI", client =>
    {
        var VillaURI = SD.ApiBaseURL;
        client.BaseAddress = new Uri(VillaURI);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
//


//


//

//
    builder.Services.AddScoped<IVillaService, VillaService>();
    builder.Services.AddScoped<ITokenProvider, TokenProvider>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IVillaAmenitiesService, VillaAmenitiesService>();
    builder.Services.AddScoped<IBookingVilla, BookingVillaService>();
    builder.Services.AddScoped<IGuest, GuestService>();
    //builder.Services.AddAuthorization();
    var app = builder.Build();
    
    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    
    app.UseSession();
    
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapStaticAssets();
    
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();
    
    
    app.Run();
    