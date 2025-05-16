using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Models.Requests;
using BookIt.BLL.Models.Responses;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstablishmentsController : ControllerBase
{
    private readonly IEstablishmentsService _service;

    public EstablishmentsController(IEstablishmentsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EstablishmentResponse>>> GetAllAsync()
    {
        var establishments = await _service.GetAllAsync();
        return Ok(establishments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstablishmentResponse>> GetByIdAsync(int id)
    {
        var establishment = await _service.GetByIdAsync(id);
        return establishment is not null ? Ok(establishment) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<EstablishmentResponse>> CreateAsync([FromBody] EstablishmentRequest request)
    {
        var establishment = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = establishment.Id }, establishment);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] EstablishmentRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
