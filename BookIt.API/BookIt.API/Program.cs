using BookIt.API.Mapping;
using BookIt.API.Middleware;
using BookIt.API.Middleware.Logging;
using BookIt.API.Models.Responses;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Services;
using BookIt.DAL.Configuration;
using BookIt.DAL.Database;
using BookIt.DAL.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;
using System.Text;

const string CORS_POLICY_NAME = "CORS_ANY";

var builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Services.ConfigureSettings(builder.Configuration);

var logFolder = builder.Configuration["LogSettings:LogFolder"] ?? "Logs";
var retainedFileCount = builder.Configuration.GetValue("LogSettings:RetainedFileCountLimit", 30);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.With<CallerEnricher>()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? []
            );

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Error = "Model Validation Failed",
            Message = "One or more model validation errors occurred",
            ErrorCode = "MODEL_VALIDATION_ERROR",
            Details = new Dictionary<string, object> { { "modelValidationErrors", errors } }
        };

        return new BadRequestObjectResult(errorResponse);
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(CORS_POLICY_NAME, p =>
            p.WithOrigins
            (
                Environment.GetEnvironmentVariable("CLIENT_URL") ?? string.Empty,
                Environment.GetEnvironmentVariable("CLIENT_URL")?.Replace("http", "https") ?? string.Empty,
                Environment.GetEnvironmentVariable("CLIENT_URL")?.Replace("https", "http") ?? string.Empty
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = "Oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string> ()
        }
    });
});

builder.Services.AddDbContext<BookingDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddMapping();
builder.Services.AddHttpClient();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

builder.Services.AddScoped<EstablishmentsRepository>();
builder.Services.AddScoped<IEstablishmentsService, EstablishmentsService>();

builder.Services.AddScoped<IClassificationService, ClassificationService>();

builder.Services.AddScoped<GeolocationRepository>();
builder.Services.AddScoped<IGeolocationService, GeolocationService>();

builder.Services.AddScoped<ApartmentsRepository>();
builder.Services.AddScoped<IApartmentsService, ApartmentsService>();

builder.Services.AddScoped<BookingsRepository>();
builder.Services.AddScoped<IBookingsService, BookingsService>();

builder.Services.AddScoped<UserRatingRepository>();
builder.Services.AddScoped<ApartmentRatingRepository>();
builder.Services.AddScoped<IRatingsService, RatingsService>();

builder.Services.AddScoped<ReviewsRepository>();
builder.Services.AddScoped<IReviewsService, ReviewsService>();

builder.Services.AddScoped<FavoritesRepository>();
builder.Services.AddScoped<IFavoritesService, FavoritesService>();

builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddHttpClient<IMonobankAcquiringService, MonobankAcquiringService>();

builder.Services.AddScoped<ImagesRepository>();
builder.Services.AddScoped<IImagesService, ImagesService>();
builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? builder.Configuration["JWT:Secret"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!))
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(CORS_POLICY_NAME);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting BookIt API application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BookIt API application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}