using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Models.Geocoding;
using BookIt.DAL.Configuration.Settings;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BookIt.BLL.Services;

public class GeolocationService : IGeolocationService, IDisposable
{
    private const double _closeGeoThreshold = 1e-5;

    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient = new();
    private readonly string _reverseGeocodingBaseUrl;
    private readonly GeolocationRepository _repository;
    private readonly GeocodingSettings _geocodingSettings;

    public GeolocationService(
        IMapper mapper,
        GeolocationRepository repository,
        IOptions<GeocodingSettings> geocodingSettingsOptions)
    {
        _geocodingSettings = geocodingSettingsOptions?.Value ?? throw new ArgumentNullException(nameof(geocodingSettingsOptions));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        ValidateGeocodingConfiguration();

        _reverseGeocodingBaseUrl = string.Join("/", [_geocodingSettings.BaseUrl, "reverse"]);

        ConfigureHttpClient();
    }

    public async Task<GeolocationDTO?> CreateAsync(GeolocationDTO dto)
    {
        try
        {
            ValidateGeolocationData(dto);

            var reverseGeocodingResult = await ReverseGeocodeAsync(dto);
            if (reverseGeocodingResult is null)
                throw new ExternalServiceException("Geocoding", "Failed to get geocoding data for the provided coordinates");

            var geolocation = _mapper.Map<Geolocation>(reverseGeocodingResult);
            var addedGeolocation = await _repository.AddAsync(geolocation);

            return _mapper.Map<GeolocationDTO>(addedGeolocation);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Geolocation", "Failed to create geolocation", ex);
        }
    }

    public async Task<GeolocationDTO?> UpdateEstablishmentGeolocationAsync(int establishmentId, GeolocationDTO dto)
    {
        try
        {
            ValidateGeolocationData(dto);

            var currentGeolocation = await _repository.GetByEstablishmentIdAsync(establishmentId);

            if (currentGeolocation is not null &&
                IsNewGeolocationCloseToCurrent((currentGeolocation.Latitude, currentGeolocation.Longitude),
                                               (dto.Latitude, dto.Longitude)))
            {
                return _mapper.Map<GeolocationDTO>(currentGeolocation);
            }

            var reverseGeocodingResult = await ReverseGeocodeAsync(dto);
            if (reverseGeocodingResult is null)
                throw new ExternalServiceException("Geocoding", "Failed to get geocoding data for the provided coordinates");

            var geolocation = _mapper.Map<Geolocation>(reverseGeocodingResult);

            Geolocation? newGeolocation;

            if (currentGeolocation?.Id is not null)
            {
                geolocation.Id = currentGeolocation.Id;
                newGeolocation = await _repository.UpdateAsync(geolocation);
            }
            else
            {
                newGeolocation = await _repository.AddAsync(geolocation);
            }

            return _mapper.Map<GeolocationDTO>(newGeolocation);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Geolocation", "Failed to update establishment geolocation", ex);
        }
    }

    public async Task<bool> DeleteEstablishmentGeolocationAsync(int establishmentId)
    {
        try
        {
            await _repository.DeleteByEstablishmentIdAsync(establishmentId);

            return true;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to delete establishment geolocation", ex);
        }
    }

    private void ValidateGeocodingConfiguration()
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(_geocodingSettings.BaseUrl))
            validationErrors.Add("BaseUrl", new List<string> { "Geocoding base URL is required" });

        if (string.IsNullOrWhiteSpace(_geocodingSettings.Host))
            validationErrors.Add("Host", new List<string> { "Geocoding host is required" });

        if (string.IsNullOrWhiteSpace(_geocodingSettings.ApiKey))
            validationErrors.Add("ApiKey", new List<string> { "Geocoding API key is required" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private void ValidateGeolocationData(GeolocationDTO dto)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (dto.Latitude < -90 || dto.Latitude > 90)
            validationErrors.Add("Latitude", new List<string> { "Latitude must be between -90 and 90 degrees" });

        if (dto.Longitude < -180 || dto.Longitude > 180)
            validationErrors.Add("Longitude", new List<string> { "Longitude must be between -180 and 180 degrees" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private void ConfigureHttpClient()
    {
        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // 30 second timeout
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookIt-Application/1.0");
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("HttpClient", "Failed to configure HTTP client for geocoding", ex);
        }
    }

    private async Task<ReverseGeocodingResult?> ReverseGeocodeAsync(GeolocationDTO dto)
    {
        try
        {
            var geocodingString = await GetGeocodingStringAsync(dto.Latitude, dto.Longitude);
            if (string.IsNullOrEmpty(geocodingString))
                return null;

            var reverseGeocodingResult = JsonSerializer.Deserialize<ReverseGeocodingResult>(geocodingString);
            if (reverseGeocodingResult is null)
                return null;

            reverseGeocodingResult.Latitude = reverseGeocodingResult.Latitude.Replace(".", ",");
            reverseGeocodingResult.Longitude = reverseGeocodingResult.Longitude.Replace(".", ",");

            return reverseGeocodingResult;
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException("Geocoding", "Failed to parse geocoding service response", ex);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Geocoding", "Reverse geocoding operation failed", ex);
        }
    }

    private async Task<string?> GetGeocodingStringAsync(double lat, double lon)
    {
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

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException("Geocoding", "Network error while calling geocoding service", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ExternalServiceException("Geocoding", "Geocoding service request timed out", ex);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Geocoding", "Unexpected error while calling geocoding service", ex);
        }
    }

    private string BuildQueryString(ReverseGeocodingOptions options)
    {
        try
        {
            return string.Join("&",
                   options.GetType()
                          .GetProperties()
                          .Select(p => $"{p.Name.ToLower()}={p.GetValue(options)}"))
                   .Replace(",", ".");
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Geocoding", "Failed to build query string for geocoding request", ex);
        }
    }

    private bool IsNewGeolocationCloseToCurrent((double lat, double lon) currentGeo, (double lat, double lon) newGeo)
    {
        try
        {
            return Math.Abs(currentGeo.lat - newGeo.lat) < _closeGeoThreshold &&
                   Math.Abs(currentGeo.lon - newGeo.lon) < _closeGeoThreshold;
        }
        catch (Exception ex)
        {
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