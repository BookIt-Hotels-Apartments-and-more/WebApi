using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Interfaces;
using BookIt.BLL.DTOs;
using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.API.Models.Requests;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IReviewsService _service;

    public ReviewsController(IMapper mapper, IReviewsService service)
    {
        _mapper = mapper;
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetAllAsync()
    {
        var reviewsDto = await _service.GetAllAsync();
        var reviewsResponse = _mapper.Map<IEnumerable<ReviewResponse>>(reviewsDto);
        return Ok(reviewsResponse);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReviewResponse>> GetByIdAsync([FromRoute] int id)
    {
        var reviewDto = await _service.GetByIdAsync(id);
        var reviewResponse = _mapper.Map<ReviewResponse>(reviewDto);
        return reviewResponse is not null ? Ok(reviewResponse) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<ReviewResponse>> CreateAsync([FromBody] ReviewRequest request)
    {
        var reviewDto = _mapper.Map<ReviewDTO>(request);
        var added = await _service.CreateAsync(reviewDto);
        if (added is null) return BadRequest("Failed to create review.");
        var reviewResponse = _mapper.Map<ReviewResponse>(added);
        return Ok(reviewResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReviewResponse>> UpdateAsync([FromRoute] int id, [FromBody] ReviewRequest request)
    {
        var reviewDto = _mapper.Map<ReviewDTO>(request);
        var updated = await _service.UpdateAsync(id, reviewDto);
        if (updated is null) return NotFound();
        var reviewResponse = _mapper.Map<ReviewResponse>(updated);
        return Ok(reviewResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
