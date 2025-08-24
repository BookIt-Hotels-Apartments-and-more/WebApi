using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tenant,Landlord,Admin")]
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
    [Authorize(Roles = "Admin")]
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
        return Ok(bookingResponse);
    }

    [HttpPost]
    [Authorize(Roles = "Tenant")]
    public async Task<ActionResult<BookingResponse>> CreateAsync([FromBody] BookingRequest request)
    {
        var bookingDto = _mapper.Map<BookingDTO>(request);
        var addedBookingDto = await _service.CreateAsync(bookingDto);
        var bookingResponse = _mapper.Map<BookingResponse>(addedBookingDto);
        return Ok(bookingResponse);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Tenant,Landlord")]
    public async Task<ActionResult<BookingResponse>> UpdateAsync([FromRoute] int id, [FromBody] BookingRequest request)
    {
        var bookingDto = _mapper.Map<BookingDTO>(request);
        var updatedBookingDto = await _service.UpdateAsync(id, bookingDto);
        var bookingResponse = _mapper.Map<BookingResponse>(updatedBookingDto);
        return Ok(bookingResponse);
    }

    [HttpPatch("check-in/{id:int}")]
    [Authorize(Roles = "Landlord")]
    public async Task<ActionResult<BookingResponse>> CheckInAsync([FromRoute] int id)
    {
        var updatedBookingDto = await _service.CheckInAsync(id);
        var bookingResponse = _mapper.Map<BookingResponse>(updatedBookingDto);
        return Ok(bookingResponse);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Tenant,Landlord")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("apartment/{apartmentId:int}/availability")]
    public async Task<ActionResult<ApartmentAvailabilityDTO>> GetApartmentAvailability([FromRoute] int apartmentId,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.Date;
        endDate ??= DateTime.UtcNow.Date.AddMonths(12);

        var availability = await _service.GetApartmentAvailabilityAsync(apartmentId, startDate, endDate);
        return Ok(availability);
    }

    [HttpGet("apartment/{apartmentId:int}/check-availability")]
    public async Task<ActionResult> CheckAvailability([FromRoute] int apartmentId,
        [FromQuery, Required] DateTime dateFrom, [FromQuery, Required] DateTime dateTo)
    {
        var isAvailable = await _service.CheckAvailabilityAsync(apartmentId, dateFrom, dateTo);
        var response = new AvailabilityCheckResponse
        {
            ApartmentId = apartmentId,
            IsAvailable = isAvailable,
            DateFrom = dateFrom,
            DateTo = dateTo
        };
        return Ok(response);
    }
}
