using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BookIt.DAL.Database;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Services;
using BookIt.DAL.Repositories;
using BookIt.API.Mapping;
using BookIt.BLL.Configuration;

const string FRONTEND_CORS_POLICY = "AcceptFrontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(FRONTEND_CORS_POLICY, policy =>
    {
        policy.WithOrigins
        (
            "http://localhost:5173",
            "https://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); 
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddDbContext<BookingDbContext>
    (opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMapping();
builder.Services.AddHttpClient();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IJWTService, JWTService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();

builder.Services.AddScoped<EstablishmentsRepository>();
builder.Services.AddScoped<IEstablishmentsService, EstablishmentsService>();

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

builder.Services.Configure<MonobankSettings>(builder.Configuration.GetSection("Monobank"));
builder.Services.AddHttpClient<IMonobankAcquiringService, MonobankAcquiringService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(FRONTEND_CORS_POLICY);

app.UseAuthorization();

app.MapControllers();

app.Run();
