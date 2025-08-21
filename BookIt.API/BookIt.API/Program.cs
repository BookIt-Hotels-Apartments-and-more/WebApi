using BookIt.API.Extensions;
using BookIt.API.Mapping;
using BookIt.API.Middleware;
using BookIt.API.Middleware.Logging;
using BookIt.API.Validation;
using BookIt.BLL.Extensions;
using BookIt.DAL.Configuration;
using BookIt.DAL.Database;
using BookIt.DAL.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSettings(builder.Configuration);

builder.Host.ConfigureSerilog(builder.Configuration);

builder.Services.ConfigureInvalidModelBehavior();

(var _, var cspName) = builder.Services.AddCorsSecurityPolicy();

builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGenWithSecurityConfiguration();
}

builder.Services.AddCustomDbContext<BookingDbContext>(builder.Configuration);
builder.Services.AddMapping();
builder.Services.AddHttpClient();
builder.Services.AddDALRepositories();
builder.Services.AddBLLServices();

builder.Services.AddJwtBearerAuthentication(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors(cspName);
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