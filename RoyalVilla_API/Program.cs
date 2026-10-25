using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services;
using RoyalVilla_API.Services.IServices;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtSettings")["Secret"]);


builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddApiVersioning(options =>
{

    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.ReportApiVersions = true; //flag to report

}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddCors();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(Options =>
{
    Options.RequireHttpsMetadata = false;
    Options.SaveToken = true;
    Options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

builder.Services.AddControllers();

var Apiprovider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

foreach (var description in Apiprovider.ApiVersionDescriptions)
{
    var versionName = description.GroupName;
    var versionNumber = description.ApiVersion.ToString();
    var displayName = $"Demo API -- + {versionName}";


    builder.Services.AddOpenApi(versionName,options =>
    {
        options.AddDocumentTransformer((document, context, CancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Demo Royal Api",
                Version = versionName,
                Description = displayName,
                Contact = new OpenApiContact
                {
                    Name = "Vignesh",
                    Email = "vignesh@gmail.com"
                }

            };

            document.Components ??= new();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter JWT Bearer token"
                }
            };

            document.Security =
            [
                new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
            }
            ];

            return Task.CompletedTask;
        });
    });
}

//builder.Services.AddOpenApi("v1");
//builder.Services.AddOpenApi("v2");


builder.Services.AddAutoMapper(o => {
     o.CreateMap<Villa, VillaCreateDTO>().ReverseMap();
     o.CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
     o.CreateMap<Villa, VillaDTO>().ReverseMap();
    o.CreateMap<VillaUpdateDTO, VillaDTO>().ReverseMap();
    o.CreateMap<ApplicationUser, UserDTO>().ReverseMap();
    o.CreateMap<User, RegisterationRequestDTO>().ReverseMap();
    o.CreateMap<VillaAmenities, VillaAmentiesDTO>().ReverseMap();
    o.CreateMap<VillaAmenities, VillaAmentiesCreateDTO>().ReverseMap();
    o.CreateMap<VillaAmenities, VillaAmentiesUpdateDTO>().ReverseMap();
    o.CreateMap<VillaAmenities, VillaAmentiesDTO>();
    o.CreateMap<VillaAmenities, VillaAmentiesDTO>()
    .ForMember(dist=>dist.VillaName, opt=>opt.MapFrom(src => src.villa!=null? src.villa.Name : null));
    o.CreateMap<VillaAmentiesDTO, VillaAmenities>();
    o.CreateMap<BookVilla, BookVillaDTO>().ReverseMap();
    o.CreateMap<BookingVillaDTO, BookVillaDTO>().ReverseMap();
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBookVillaService, BookVillaService>();


var app = builder.Build();
await SeedDataAsync(app);

// Configure the HTTP request pipeline.
    app.MapOpenApi("/openapi/{documentName}.json");
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.MapScalarApiReference( option=>
    {
        option.Title = "Demo: Royal Villa";

        var sortedVersion = provider.ApiVersionDescriptions.OrderBy(v => v.ApiVersion).ToList();
        foreach(var description in sortedVersion)
        {
            var versionName = description.GroupName;
            var versionNumber = description.ApiVersion.ToString();
            var displayName = $"Demo API -- + {versionName}";

            var isDefault = description.ApiVersion.Equals(new ApiVersion(2, 0));
            option.AddDocument(versionName, displayName, $"openapi/{versionName}.json", isDefault);
        }

        //option.AddDocument("V1", "Demo royal villa", "openapi/v1.json",isDefault: true)
        //.AddDocument("V2", "Demo royal villa", "openapi/v2.json", isDefault: false);
    }
        );

app.UseStaticFiles();
app.UseCors(o=>o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("*"));
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Database.MigrateAsync();
}//update db when we run app itself
