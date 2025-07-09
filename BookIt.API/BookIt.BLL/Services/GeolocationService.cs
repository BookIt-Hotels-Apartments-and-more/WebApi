using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Models.Geocoding;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BookIt.BLL.Services;

public class GeolocationService : IGeolocationService, IDisposable
{
    private readonly string _host;
    private readonly string _apiKey;
    private readonly string _reverseGeocodingBaseUrl;
    private readonly HttpClient _httpClient = new();

    private readonly IMapper _mapper;
    private readonly GeolocationRepository _repository;

    public GeolocationService(
        IMapper mapper,
        IConfiguration configuration,
        GeolocationRepository repository)
    {
        _host = configuration["Geocoding:Host"]!;
        _apiKey = configuration["Geocoding:ApiKey"] ?? throw new ArgumentNullException("Geocoding API key is not configured.");
        _reverseGeocodingBaseUrl = string.Join("/", [configuration["Geocoding:BaseUrl"], "reverse"]);

        _mapper = mapper;
        _repository = repository;
    }

    public async Task<GeolocationDTO?> CreateAsync(GeolocationDTO dto)
    {
        var reverseGeocodingResult = await ReverseGeocode(dto);
        if (reverseGeocodingResult is null)
            return null;

        reverseGeocodingResult.Latitude = reverseGeocodingResult.Latitude.Replace(".", ",");
        reverseGeocodingResult.Longitude = reverseGeocodingResult.Longitude.Replace(".", ",");

        var geolocation = _mapper.Map<Geolocation>(reverseGeocodingResult);

        var addedGeolocation = await _repository.AddAsync(geolocation);
        var geolocationDto = _mapper.Map<GeolocationDTO>(addedGeolocation);
        return geolocationDto;
    }

    private async Task<ReverseGeocodingResult?> ReverseGeocode(GeolocationDTO dto)
    {
        var geocodingString = await GetGeocodingString(dto.Latitude, dto.Longitude);
        if (string.IsNullOrEmpty(geocodingString))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ReverseGeocodingResult>(geocodingString);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<string?> GetGeocodingString(double lat, double lon)
    {
        var options = new ReverseGeocodingOptions { Lat = lat, Lon = lon };
        var queryParamsString = BuildQueryString(options);
        var uri = new Uri($"{_reverseGeocodingBaseUrl}?{queryParamsString}");

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        request.Headers.Add("x-rapidapi-host", _host);
        request.Headers.Add("x-rapidapi-key", _apiKey);

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private string BuildQueryString(ReverseGeocodingOptions options)
    {
        return string.Join("&",
               options.GetType()
                      .GetProperties()
                      .Select(p => $"{p.Name.ToLower()}={p.GetValue(options)}"))
               .Replace(",", ".");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
