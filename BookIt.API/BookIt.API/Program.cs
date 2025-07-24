using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BookIt.DAL.Database;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Services;
using BookIt.DAL.Repositories;
using BookIt.API.Mapping;
using DotNetEnv;
using BookIt.DAL.Configuration;

const string CORS_POLICY_NAME = "CORS_ANY";

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CORS_POLICY_NAME, p => p.AllowAnyOrigin().AllowAnyMethod().AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BookingDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddMapping();
builder.Services.AddHttpClient();

builder.Services.ConfigureSettings(builder.Configuration);

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IJWTService, JWTService>();
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(CORS_POLICY_NAME);

app.UseAuthorization();

app.MapControllers();

app.Run();
