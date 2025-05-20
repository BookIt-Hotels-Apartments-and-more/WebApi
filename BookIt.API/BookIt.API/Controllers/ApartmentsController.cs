using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Interfaces;
using BookIt.BLL.DTOs;
using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.API.Models.Requests;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApartmentResponse>> GetByIdAsync([FromRoute] int id)
    {
        var apartmentDto = await _service.GetByIdAsync(id);
        var apartmentResponse = _mapper.Map<ApartmentResponse>(apartmentDto);
        return apartmentResponse is not null ? Ok(apartmentResponse) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<ApartmentResponse>> CreateAsync([FromBody] ApartmentRequest request)
    {
        var apartmentDto = _mapper.Map<ApartmentDTO>(request);
        var added = await _service.CreateAsync(apartmentDto);
        if (added is null) return BadRequest("Failed to create apartment.");
        var apartmentResponse = _mapper.Map<ApartmentResponse>(added);
        return Ok(apartmentResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApartmentResponse>> UpdateAsync([FromRoute] int id, [FromBody] ApartmentRequest request)
    {
        var apartmentDto = _mapper.Map<ApartmentDTO>(request);
        var updated = await _service.UpdateAsync(id, apartmentDto);
        if (updated is null) return NotFound();
        var apartmentResponse = _mapper.Map<ApartmentResponse>(updated);
        return Ok(apartmentResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
