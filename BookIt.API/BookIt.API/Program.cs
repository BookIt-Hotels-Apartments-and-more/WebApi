using BookIt.API.Extensions;
using BookIt.API.Mapping;
using BookIt.API.Middleware;
using BookIt.API.Middleware.Logging;
using BookIt.API.Validation;
using BookIt.BLL.Extensions;
using BookIt.DAL.Configuration;
using BookIt.DAL.Database;
using BookIt.DAL.Extensions;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSettings(builder.Configuration);

builder.Host.ConfigureSerilog(builder.Configuration);

builder.Services.ConfigureInvalidModelBehavior();

(var _, var cspName) = builder.Services.AddCorsSecurityPolicy();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGenWithSecurityConfiguration();

builder.Services.AddRedisCache();
builder.Services.AddCustomDbContext<BookingDbContext>();
builder.Services.AddMapping();
builder.Services.AddBLLServices();
builder.Services.AddDALRepositories();
builder.Services.AddCustomHttpClients();

builder.Services.AddJwtBearerAuthentication(builder.Configuration);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(cspName);
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCompression();
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