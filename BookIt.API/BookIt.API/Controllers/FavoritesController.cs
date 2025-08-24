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
public class FavoritesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IFavoritesService _service;

    public FavoritesController(IMapper mapper, IFavoritesService service)
    {
        _mapper = mapper;
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<FavoriteResponse>>> GetAllAsync()
    {
        var favoritesDto = await _service.GetAllAsync();
        var favoritesResponse = _mapper.Map<IEnumerable<FavoriteResponse>>(favoritesDto);
        return Ok(favoritesResponse);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FavoriteResponse>> GetByIdAsync([FromRoute] int id)
    {
        var favoriteDto = await _service.GetByIdAsync(id);
        var favoriteResponse = _mapper.Map<FavoriteResponse>(favoriteDto);
        return Ok(favoriteResponse);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<FavoriteResponse>>> GetAllForMeAsync()
    {
        var requestorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(requestorIdStr)) return Unauthorized();
        if (!int.TryParse(requestorIdStr, out var requestorId)) return Unauthorized();

        var favoritesDto = await _service.GetAllForUserAsync(requestorId);
        var favoritesResponse = _mapper.Map<IEnumerable<FavoriteResponse>>(favoritesDto);
        return Ok(favoritesResponse);
    }

    [HttpGet("establishment/{establishmentId:int}")]
    public async Task<ActionResult<int>> GetCountByEstablishmentIdAsync([FromRoute] int establishmentId)
    {
        var favoritesCount = await _service.GetCountForEstablishmentAsync(establishmentId);
        return Ok(favoritesCount);
    }

    [HttpPost]
    public async Task<ActionResult<FavoriteResponse>> CreateAsync([FromBody] FavoriteRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();
        if (request.UserId is not null && request.UserId != userId) return Forbid("You can only create favorites for yourself.");

        request.UserId = userId;

        var favoriteDto = _mapper.Map<FavoriteDTO>(request);
        var added = await _service.CreateAsync(favoriteDto);
        if (added is null) return BadRequest("Failed to create favorite.");
        var favoriteResponse = _mapper.Map<FavoriteResponse>(added);
        return Ok(favoriteResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var requestorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(requestorIdStr)) return Unauthorized();
        if (!int.TryParse(requestorIdStr, out var requestorId)) return Unauthorized();

        await _service.DeleteAsync(id, requestorId);
        return NoContent();
    }
}
