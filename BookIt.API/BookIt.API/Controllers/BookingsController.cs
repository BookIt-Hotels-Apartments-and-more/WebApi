using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        try
        {
            var bookingDto = _mapper.Map<BookingDTO>(request);
            var added = await _service.CreateAsync(bookingDto);
            if (added is null) return BadRequest("Failed to create booking.");
            var bookingResponse = _mapper.Map<BookingResponse>(added);
            return Ok(bookingResponse);
        }
        catch (BookingConflictException ex)
        {
            return Conflict(new
            {
                message = ex.Message,
                apartmentId = ex.ApartmentId,
                requestedDateFrom = ex.DateFrom,
                requestedDateTo = ex.DateTo,
                conflictingBookings = ex.ConflictingBookings
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookingResponse>> UpdateAsync([FromRoute] int id, [FromBody] BookingRequest request)
    {
        try
        {
            var bookingDto = _mapper.Map<BookingDTO>(request);
            var updated = await _service.UpdateAsync(id, bookingDto);
            if (updated is null) return NotFound();
            var bookingResponse = _mapper.Map<BookingResponse>(updated);
            return Ok(bookingResponse);
        }
        catch (BookingConflictException ex)
        {
            return Conflict(new
            {
                message = ex.Message,
                apartmentId = ex.ApartmentId,
                requestedDateFrom = ex.DateFrom,
                requestedDateTo = ex.DateTo,
                conflictingBookings = ex.ConflictingBookings
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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

    [HttpGet("apartment/{apartmentId}/availability")]
    public async Task<ActionResult<ApartmentAvailabilityDTO>> GetApartmentAvailability(
        int apartmentId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            startDate ??= DateTime.UtcNow.Date;
            endDate ??= DateTime.UtcNow.Date.AddMonths(12);

            if (startDate >= endDate)
                return BadRequest("Start date must be before end date");

            var availability = await _service.GetApartmentAvailabilityAsync(apartmentId, startDate, endDate);
            return Ok(availability);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("apartment/{apartmentId:int}/check-availability")]
    public async Task<ActionResult> CheckAvailability(
        int apartmentId,
        [FromQuery, Required] DateTime dateFrom,
        [FromQuery, Required] DateTime dateTo)
    {
        if (dateFrom >= dateTo) return BadRequest("Check-in date must be before check-out date");

        var isAvailable = await _service.CheckAvailabilityAsync(apartmentId, dateFrom, dateTo);

        return Ok(new
        {
            apartmentId,
            dateFrom,
            dateTo,
            isAvailable,
        });
    }
}
