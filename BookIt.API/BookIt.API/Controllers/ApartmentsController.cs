using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Models.Requests;
using BookIt.BLL.Models.Responses;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApartmentsController : ControllerBase
{
    private readonly IApartmentsService _service;

    public ApartmentsController(IApartmentsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApartmentResponse>>> GetAllAsync()
    {
        var apartments = await _service.GetAllAsync();
        return Ok(apartments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApartmentResponse>> GetByIdAsync([FromRoute] int id)
    {
        var apartment = await _service.GetByIdAsync(id);
        return apartment is not null ? Ok(apartment) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<EstablishmentResponse>> CreateAsync([FromBody] ApartmentRequest request)
    {
        var added = await _service.CreateAsync(request);
        return added is null ? BadRequest("Failed to create apartment.") : Ok(added);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateAsync([FromRoute] int id, [FromBody] ApartmentRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
