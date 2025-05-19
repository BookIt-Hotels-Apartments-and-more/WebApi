using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Interfaces;
using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.API.Models.Responses;
using BookIt.API.Models.Requests;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EstablishmentResponse>>> GetAllAsync()
    {
        var establishmentsDto = await _service.GetAllAsync();
        var establishmentsResponse = _mapper.Map<IEnumerable<EstablishmentResponse>>(establishmentsDto);
        return Ok(establishmentsResponse);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstablishmentResponse>> GetByIdAsync([FromRoute] int id)
    {
        var establishmentDto = await _service.GetByIdAsync(id);
        var establishmentResponse = _mapper.Map<EstablishmentResponse>(establishmentDto);
        return establishmentResponse is not null ? Ok(establishmentResponse) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<EstablishmentResponse>> CreateAsync([FromBody] EstablishmentRequest request)
    {
        var establishmentDto = _mapper.Map<EstablishmentDTO>(request);
        var added = await _service.CreateAsync(establishmentDto);
        if (added is null) return BadRequest("Failed to create establishment.");
        var establishmentResponse = _mapper.Map<EstablishmentResponse>(added);
        return Ok(establishmentResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EstablishmentResponse>> UpdateAsync([FromRoute] int id, [FromBody] EstablishmentRequest request)
    {
        var establishmentDto = _mapper.Map<EstablishmentDTO>(request);
        var updated = await _service.UpdateAsync(id, establishmentDto);
        if (updated is null) return NotFound();
        var establishmentResponse = _mapper.Map<EstablishmentResponse>(updated);
        return Ok(establishmentResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
