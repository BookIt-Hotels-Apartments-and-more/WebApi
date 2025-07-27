using AutoMapper;
using BookIt.BLL.DTOs;
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
    private readonly IOptions<GeocodingSettings> _geocodingSettingsOptions;

    public GeolocationService(
        IMapper mapper,
        GeolocationRepository repository,
        IOptions<GeocodingSettings> geocodingSettingsOptions)
    {
        _geocodingSettingsOptions = geocodingSettingsOptions;
        _reverseGeocodingBaseUrl = string.Join("/", [geocodingSettingsOptions.Value.BaseUrl, "reverse"]);

        _mapper = mapper;
        _repository = repository;
    }

    public async Task<GeolocationDTO?> CreateAsync(GeolocationDTO dto)
    {
        var reverseGeocodingResult = await ReverseGeocode(dto);
        if (reverseGeocodingResult is null)
            return null;

        var geolocation = _mapper.Map<Geolocation>(reverseGeocodingResult);

        var addedGeolocation = await _repository.AddAsync(geolocation);
        var geolocationDto = _mapper.Map<GeolocationDTO>(addedGeolocation);
        return geolocationDto;
    }

    public async Task<GeolocationDTO?> UpdateEstablishmentGeolocationAsync(int establishmentId, GeolocationDTO dto)
    {
        var currentGeolocation = await _repository.GetByEstablishmentIdAsync(establishmentId);

        if (currentGeolocation is not null &&
            IsNewGeolocationCloseToCurrent((currentGeolocation.Latitude, currentGeolocation.Longitude),
                                           (dto.Latitude, dto.Longitude)))
        {
            var geolocationDto = _mapper.Map<GeolocationDTO>(currentGeolocation);
            return geolocationDto;
        }

        var reverseGeocodingResult = await ReverseGeocode(dto);
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

        var newGeolocationDto = _mapper.Map<GeolocationDTO>(newGeolocation);
        return newGeolocationDto;
    }

    public async Task<bool> DeleteEstablishmentGeolocationAsync(int establishmentId)
    {
        try
        {
            await _repository.DeleteByEstablishmentIdAsync(establishmentId);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<ReverseGeocodingResult?> ReverseGeocode(GeolocationDTO dto)
    {
        var geocodingString = await GetGeocodingString(dto.Latitude, dto.Longitude);
        if (string.IsNullOrEmpty(geocodingString))
            return null;

        try
        {
            var reverseGeocodingResult = JsonSerializer.Deserialize<ReverseGeocodingResult>(geocodingString);
            if (reverseGeocodingResult is null) return null;

            reverseGeocodingResult.Latitude = reverseGeocodingResult.Latitude.Replace(".", ",");
            reverseGeocodingResult.Longitude = reverseGeocodingResult.Longitude.Replace(".", ",");

            return reverseGeocodingResult;
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

        request.Headers.Add("x-rapidapi-host", _geocodingSettingsOptions.Value.Host);
        request.Headers.Add("x-rapidapi-key", _geocodingSettingsOptions.Value.ApiKey);

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

    private bool IsNewGeolocationCloseToCurrent((double lat, double lon) currentGeo, (double lat, double lon) newGeo)
    {
        return Math.Abs(currentGeo.lat - newGeo.lat) < _closeGeoThreshold &&
               Math.Abs(currentGeo.lon - newGeo.lon) < _closeGeoThreshold;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
