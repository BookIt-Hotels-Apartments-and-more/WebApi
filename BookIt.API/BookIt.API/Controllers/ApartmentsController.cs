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
[Authorize(Roles = "Tenant,Landlord,Admin")]
public class ApartmentsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IApartmentsService _service;

    public ApartmentsController(IMapper mapper, IApartmentsService service)
    {
        _mapper = mapper;
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApartmentResponse>>> GetAllAsync()
    {
        var apartmentsDto = await _service.GetAllAsync();
        var apartmentsResponse = _mapper.Map<IEnumerable<ApartmentResponse>>(apartmentsDto);
        return Ok(apartmentsResponse);
    }

    [HttpGet("establishment/{establishmentId:int}")]
    public async Task<ActionResult<PaginatedResponse<ApartmentResponse>>> GetPagedByEstablishmentIdAsync([FromRoute] int establishmentId, [FromQuery] PaginationRequest request)
    {
        var pagedResult = await _service.GetPagedByEstablishmentIdAsync(establishmentId, request.Page, request.PageSize);
        var response = _mapper.Map<PaginatedResponse<ApartmentResponse>>(pagedResult);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApartmentResponse>> GetByIdAsync([FromRoute] int id)
    {
        var apartmentDto = await _service.GetByIdAsync(id);
        var apartmentResponse = _mapper.Map<ApartmentResponse>(apartmentDto);
        return Ok(apartmentResponse);
    }

    [HttpPost]
    [Authorize(Roles = "Landlord,Admin")]
    public async Task<ActionResult<ApartmentResponse>> CreateAsync([FromBody] ApartmentRequest request)
    {
        var requestorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(requestorIdStr)) return Unauthorized();
        if (!int.TryParse(requestorIdStr, out var requestorId)) return Unauthorized();

        var apartmentDto = _mapper.Map<ApartmentDTO>(request);
        var added = await _service.CreateAsync(apartmentDto, requestorId);
        var apartmentResponse = _mapper.Map<ApartmentResponse>(added);
        return Ok(apartmentResponse);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Landlord,Admin")]
    public async Task<ActionResult<ApartmentResponse>> UpdateAsync([FromRoute] int id, [FromBody] ApartmentRequest request)
    {
        var requestorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(requestorIdStr)) return Unauthorized();
        if (!int.TryParse(requestorIdStr, out var requestorId)) return Unauthorized();

        var apartmentDto = _mapper.Map<ApartmentDTO>(request);
        var updated = await _service.UpdateAsync(id, apartmentDto, requestorId);
        var apartmentResponse = _mapper.Map<ApartmentResponse>(updated);
        return Ok(apartmentResponse);
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