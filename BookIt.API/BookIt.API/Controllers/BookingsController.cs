using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Interfaces;
using BookIt.BLL.DTOs;
using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.API.Models.Requests;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IBookingsService _service;

    public BookingsController(IMapper mapper, IBookingsService service)
    {
        _mapper = mapper;
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingResponse>>> GetAllAsync()
    {
        var bookingsDto = await _service.GetAllAsync();
        var bookingsResponse = _mapper.Map<IEnumerable<BookingResponse>>(bookingsDto);
        return Ok(bookingsResponse);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetByIdAsync([FromRoute] int id)
    {
        var bookingDto = await _service.GetByIdAsync(id);
        var bookingResponse = _mapper.Map<BookingResponse>(bookingDto);
        return bookingResponse is not null ? Ok(bookingResponse) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> CreateAsync([FromBody] BookingRequest request)
    {
        var bookingDto = _mapper.Map<BookingDTO>(request);
        var added = await _service.CreateAsync(bookingDto);
        if (added is null) return BadRequest("Failed to create booking.");
        var bookingResponse = _mapper.Map<BookingResponse>(added);
        return Ok(bookingResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookingResponse>> UpdateAsync([FromRoute] int id, [FromBody] BookingRequest request)
    {
        var bookingDto = _mapper.Map<BookingDTO>(request);
        var updated = await _service.UpdateAsync(id, bookingDto);
        if (updated is null) return NotFound();
        var bookingResponse = _mapper.Map<BookingResponse>(updated);
        return Ok(bookingResponse);
    }

    [HttpPatch("check-in/{id:int}")]
    public async Task<ActionResult<BookingResponse>> CheckInAsync([FromRoute] int id)
    {
        var updated = await _service.CheckInAsync(id);
        if (updated is null) return NotFound();
        var bookingResponse = _mapper.Map<BookingResponse>(updated);
        return Ok(bookingResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
