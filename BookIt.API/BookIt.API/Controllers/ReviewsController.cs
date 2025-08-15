using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("filter")]
    public async Task<ActionResult<PaginatedResponse<ReviewResponse>>> GetFilteredAsync([FromQuery] ReviewFilterRequest request)
    {
        var filterDto = _mapper.Map<ReviewFilterDTO>(request);
        var pagedResult = await _service.GetFilteredAsync(filterDto);
        var response = _mapper.Map<PaginatedResponse<ReviewResponse>>(pagedResult);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReviewResponse>> GetByIdAsync([FromRoute] int id)
    {
        var reviewDto = await _service.GetByIdAsync(id);
        var reviewResponse = _mapper.Map<ReviewResponse>(reviewDto);
        return Ok(reviewResponse);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewResponse>> CreateAsync([FromBody] ReviewRequest request)
    {
        var reviewDto = _mapper.Map<ReviewDTO>(request);
        var added = await _service.CreateAsync(reviewDto);
        var reviewResponse = _mapper.Map<ReviewResponse>(added);
        return Ok(reviewResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReviewResponse>> UpdateAsync([FromRoute] int id, [FromBody] ReviewRequest request)
    {
        var reviewDto = _mapper.Map<ReviewDTO>(request);
        var updated = await _service.UpdateAsync(id, reviewDto);
        var reviewResponse = _mapper.Map<ReviewResponse>(updated);
        return Ok(reviewResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
