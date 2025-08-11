using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Models.Geocoding;
using BookIt.DAL.Configuration.Settings;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BookIt.BLL.Services;

public class GeolocationService : IGeolocationService, IDisposable
{
    private const double _closeGeoThreshold = 1e-5;

    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient = new();
    private readonly string _reverseGeocodingBaseUrl;
    private readonly GeolocationRepository _repository;
    private readonly ILogger<GeolocationService> _logger;
    private readonly GeocodingSettings _geocodingSettings;

    public GeolocationService(
        IMapper mapper,
        GeolocationRepository repository,
        ILogger<GeolocationService> logger,
        IOptions<GeocodingSettings> geocodingSettingsOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _geocodingSettings = geocodingSettingsOptions?.Value ?? throw new ArgumentNullException(nameof(geocodingSettingsOptions));

        _logger.LogInformation("Initializing GeolocationService");

        ValidateGeocodingConfiguration();

        _reverseGeocodingBaseUrl = string.Join("/", [_geocodingSettings.BaseUrl, "reverse"]);

        ConfigureHttpClient();
    }

    public async Task<GeolocationDTO?> CreateAsync(GeolocationDTO dto)
    {
        _logger.LogInformation("CreateAsync called with coordinates: Latitude={Latitude}, Longitude={Longitude}", dto.Latitude, dto.Longitude);
        try
        {
            ValidateGeolocationData(dto);

            var reverseGeocodingResult = await ReverseGeocodeAsync(dto);
            if (reverseGeocodingResult is null)
            {
                _logger.LogWarning("Reverse geocoding returned null for coordinates: Latitude={Latitude}, Longitude={Longitude}", dto.Latitude, dto.Longitude);
                throw new ExternalServiceException("Geocoding", "Failed to get geocoding data for the provided coordinates");
            }

            var geolocation = _mapper.Map<Geolocation>(reverseGeocodingResult);
            var addedGeolocation = await _repository.AddAsync(geolocation);

            _logger.LogInformation("Created geolocation with Id {Id}", addedGeolocation.Id);

            return _mapper.Map<GeolocationDTO>(addedGeolocation);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create geolocation");
            throw new ExternalServiceException("Geolocation", "Failed to create geolocation", ex);
        }
    }

    public async Task<GeolocationDTO?> UpdateEstablishmentGeolocationAsync(int establishmentId, GeolocationDTO dto)
    {
        _logger.LogInformation("UpdateEstablishmentGeolocationAsync called for EstablishmentId={EstablishmentId} with coordinates: Latitude={Latitude}, Longitude={Longitude}",
            establishmentId, dto.Latitude, dto.Longitude);
        try
        {
            ValidateGeolocationData(dto);

            var currentGeolocation = await _repository.GetByEstablishmentIdAsync(establishmentId);

            if (currentGeolocation is not null &&
                IsNewGeolocationCloseToCurrent((currentGeolocation.Latitude, currentGeolocation.Longitude),
                                               (dto.Latitude, dto.Longitude)))
            {
                _logger.LogInformation("New geolocation is close to current for EstablishmentId={EstablishmentId}, skipping update", establishmentId);
                return _mapper.Map<GeolocationDTO>(currentGeolocation);
            }

            var reverseGeocodingResult = await ReverseGeocodeAsync(dto);
            if (reverseGeocodingResult is null)
            {
                _logger.LogWarning("Reverse geocoding returned null for coordinates: Latitude={Latitude}, Longitude={Longitude}", dto.Latitude, dto.Longitude);
                throw new ExternalServiceException("Geocoding", "Failed to get geocoding data for the provided coordinates");
            }

            var geolocation = _mapper.Map<Geolocation>(reverseGeocodingResult);

            Geolocation? newGeolocation;

            if (currentGeolocation?.Id is not null)
            {
                geolocation.Id = currentGeolocation.Id;
                newGeolocation = await _repository.UpdateAsync(geolocation);
                _logger.LogInformation("Updated geolocation with Id {Id} for EstablishmentId={EstablishmentId}", geolocation.Id, establishmentId);
            }
            else
            {
                newGeolocation = await _repository.AddAsync(geolocation);
                _logger.LogInformation("Added new geolocation with Id {Id} for EstablishmentId={EstablishmentId}", newGeolocation.Id, establishmentId);
            }

            return _mapper.Map<GeolocationDTO>(newGeolocation);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update establishment geolocation for EstablishmentId={EstablishmentId}", establishmentId);
            throw new ExternalServiceException("Geolocation", "Failed to update establishment geolocation", ex);
        }
    }

    public async Task<bool> DeleteEstablishmentGeolocationAsync(int establishmentId)
    {
        _logger.LogInformation("DeleteEstablishmentGeolocationAsync called for EstablishmentId={EstablishmentId}", establishmentId);
        try
        {
            await _repository.DeleteByEstablishmentIdAsync(establishmentId);
            _logger.LogInformation("Deleted geolocation for EstablishmentId={EstablishmentId}", establishmentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete establishment geolocation for EstablishmentId={EstablishmentId}", establishmentId);
            throw new ExternalServiceException("Database", "Failed to delete establishment geolocation", ex);
        }
    }

    private void ValidateGeocodingConfiguration()
    {
        _logger.LogInformation("Validating geocoding configuration");
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(_geocodingSettings.BaseUrl))
            validationErrors.Add("BaseUrl", new List<string> { "Geocoding base URL is required" });

        if (string.IsNullOrWhiteSpace(_geocodingSettings.Host))
            validationErrors.Add("Host", new List<string> { "Geocoding host is required" });

        if (string.IsNullOrWhiteSpace(_geocodingSettings.ApiKey))
            validationErrors.Add("ApiKey", new List<string> { "Geocoding API key is required" });

        if (validationErrors.Any())
        {
            _logger.LogError("Geocoding configuration validation failed: {Errors}", validationErrors);
            throw new ValidationException(validationErrors);
        }
        _logger.LogInformation("Geocoding configuration validated successfully");
    }

    private void ValidateGeolocationData(GeolocationDTO dto)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (dto.Latitude < -90 || dto.Latitude > 90)
            validationErrors.Add("Latitude", new List<string> { "Latitude must be between -90 and 90 degrees" });

        if (dto.Longitude < -180 || dto.Longitude > 180)
            validationErrors.Add("Longitude", new List<string> { "Longitude must be between -180 and 180 degrees" });

        if (validationErrors.Any())
        {
            _logger.LogWarning("Geolocation data validation failed: {Errors}", validationErrors);
            throw new ValidationException(validationErrors);
        }
    }

    private void ConfigureHttpClient()
    {
        _logger.LogInformation("Configuring HTTP client for geocoding");
        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookIt-Application/1.0");
            _logger.LogInformation("HTTP client configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure HTTP client");
            throw new ExternalServiceException("HttpClient", "Failed to configure HTTP client for geocoding", ex);
        }
    }

    private async Task<ReverseGeocodingResult?> ReverseGeocodeAsync(GeolocationDTO dto)
    {
        _logger.LogInformation("ReverseGeocodeAsync called for coordinates: Latitude={Latitude}, Longitude={Longitude}", dto.Latitude, dto.Longitude);
        try
        {
            var geocodingString = await GetGeocodingStringAsync(dto.Latitude, dto.Longitude);
            if (string.IsNullOrEmpty(geocodingString))
            {
                _logger.LogWarning("Empty geocoding response string");
                return null;
            }

            var reverseGeocodingResult = JsonSerializer.Deserialize<ReverseGeocodingResult>(geocodingString);
            if (reverseGeocodingResult is null)
            {
                _logger.LogWarning("Deserialized reverse geocoding result is null");
                return null;
            }

            reverseGeocodingResult.Latitude = reverseGeocodingResult.Latitude.Replace(".", ",");
            reverseGeocodingResult.Longitude = reverseGeocodingResult.Longitude.Replace(".", ",");

            _logger.LogInformation("Reverse geocoding successful");
            return reverseGeocodingResult;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse geocoding service response");
            throw new ExternalServiceException("Geocoding", "Failed to parse geocoding service response", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reverse geocoding operation failed");
            throw new ExternalServiceException("Geocoding", "Reverse geocoding operation failed", ex);
        }
    }

    private async Task<string?> GetGeocodingStringAsync(double lat, double lon)
    {
        _logger.LogInformation("GetGeocodingStringAsync called for Latitude={Latitude}, Longitude={Longitude}", lat, lon);
        try
        {
            var options = new ReverseGeocodingOptions { Lat = lat, Lon = lon };
            var queryParamsString = BuildQueryString(options);
            var uri = new Uri($"{_reverseGeocodingBaseUrl}?{queryParamsString}");

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.Headers.Add("x-rapidapi-host", _geocodingSettings.Host);
            request.Headers.Add("x-rapidapi-key", _geocodingSettings.ApiKey);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Geocoding service returned error {StatusCode}: {Content}", response.StatusCode, errorContent);

                throw response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => new ExternalServiceException(
                        "Geocoding", "Invalid API key or unauthorized access", null, 401),
                    System.Net.HttpStatusCode.Forbidden => new ExternalServiceException(
                        "Geocoding", "Access forbidden - check API permissions", null, 403),
                    System.Net.HttpStatusCode.TooManyRequests => new ExternalServiceException(
                        "Geocoding", "Rate limit exceeded for geocoding service", null, 429),
                    System.Net.HttpStatusCode.BadRequest => new ExternalServiceException(
                        "Geocoding", "Invalid coordinates provided to geocoding service", null, 400),
                    _ => new ExternalServiceException(
                        "Geocoding", $"Geocoding service error: {response.StatusCode}", null, (int)response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Geocoding service returned success response");
            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while calling geocoding service");
            throw new ExternalServiceException("Geocoding", "Network error while calling geocoding service", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Geocoding service request timed out");
            throw new ExternalServiceException("Geocoding", "Geocoding service request timed out", ex);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while calling geocoding service");
            throw new ExternalServiceException("Geocoding", "Unexpected error while calling geocoding service", ex);
        }
    }

    private string BuildQueryString(ReverseGeocodingOptions options)
    {
        try
        {
            var queryString = string.Join("&",
               options.GetType()
                      .GetProperties()
                      .Select(p => $"{p.Name.ToLower()}={p.GetValue(options)}"))
               .Replace(",", ".");

            _logger.LogInformation("Built query string for geocoding request: {QueryString}", queryString);

            return queryString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build query string for geocoding request");
            throw new ExternalServiceException("Geocoding", "Failed to build query string for geocoding request", ex);
        }
    }

    private bool IsNewGeolocationCloseToCurrent((double lat, double lon) currentGeo, (double lat, double lon) newGeo)
    {
        try
        {
            var isClose = Math.Abs(currentGeo.lat - newGeo.lat) < _closeGeoThreshold &&
                          Math.Abs(currentGeo.lon - newGeo.lon) < _closeGeoThreshold;

            _logger.LogInformation("Geolocation proximity check result: {IsClose}", isClose);

            return isClose;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compare geolocation coordinates");
            throw new ExternalServiceException("Geolocation", "Failed to compare geolocation coordinates", ex);
        }
    }

    public void Dispose()
    {
        try
        {
            _httpClient?.Dispose();
        }
        catch (Exception)
        { }
    }
}
