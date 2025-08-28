using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstablishmentsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IEstablishmentsService _service;

    public EstablishmentsController(IMapper mapper, IEstablishmentsService service)
    {
        _mapper = mapper;
        _service = service;
    }

    [HttpGet("filter")]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResponse<EstablishmentResponse>>> GetFilteredAsync([FromQuery] EstablishmentFilterRequest request)
    {
        var filterDto = _mapper.Map<EstablishmentFilterDTO>(request);
        var pagedResult = await _service.GetFilteredAsync(filterDto);
        var response = _mapper.Map<PaginatedResponse<EstablishmentResponse>>(pagedResult);
        return Ok(response);
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TrendingEstablishmentResponse>>> GetTrendingAsync([FromQuery] TrendingEstablishmentsRequest request)
    {
        var trendingEstablishmentsDto = await _service.GetTrendingAsync(request.Count, request.PastDays);
        var trendingEstablishmentsResponse = _mapper.Map<IEnumerable<TrendingEstablishmentResponse>>(trendingEstablishmentsDto);
        return Ok(trendingEstablishmentsResponse);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EstablishmentResponse>>> GetAllAsync()
    {
        var establishmentsDto = await _service.GetAllAsync();
        var establishmentsResponse = _mapper.Map<IEnumerable<EstablishmentResponse>>(establishmentsDto);
        return Ok(establishmentsResponse);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<EstablishmentResponse>> GetByIdAsync([FromRoute] int id)
    {
        var establishmentDto = await _service.GetByIdAsync(id);
        var establishmentResponse = _mapper.Map<EstablishmentResponse>(establishmentDto);
        return Ok(establishmentResponse);
    }

    [HttpPost]
    [Authorize(Roles = "Landlord,Admin")]
    public async Task<ActionResult<EstablishmentResponse>> CreateAsync([FromBody] EstablishmentRequest request)
    {
        var requestorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requestorRoleStr = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(requestorIdStr)) return Unauthorized();
        if (!int.TryParse(requestorIdStr, out var requestorId)) return Unauthorized();

        if (requestorRoleStr == "Landlord" && request.OwnerId != requestorId)
        {
            request.OwnerId = requestorId;
        }

        var establishmentDto = _mapper.Map<EstablishmentDTO>(request);
        var addedEstablishment = await _service.CreateAsync(establishmentDto);
        var establishmentResponse = _mapper.Map<EstablishmentResponse>(addedEstablishment);
        return Ok(establishmentResponse);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Landlord,Admin")]
    public async Task<ActionResult<EstablishmentResponse>> UpdateAsync([FromRoute] int id, [FromBody] EstablishmentRequest request)
    {
        var requestorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requestorRoleStr = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(requestorIdStr)) return Unauthorized();
        if (!int.TryParse(requestorIdStr, out var requestorId)) return Unauthorized();

        if (requestorRoleStr == "Landlord" && request.OwnerId != requestorId)
        {
            request.OwnerId = requestorId;
        }

        var establishmentDto = _mapper.Map<EstablishmentDTO>(request);
        var updatedEstablishment = await _service.UpdateAsync(id, establishmentDto);
        var establishmentResponse = _mapper.Map<EstablishmentResponse>(updatedEstablishment);
        return Ok(establishmentResponse);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Landlord,Admin")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var requestorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(requestorIdStr)) return Unauthorized();
        if (!int.TryParse(requestorIdStr, out var requestorId)) return Unauthorized();

        await _service.DeleteAsync(id, requestorId);
        return NoContent();
    }
}
