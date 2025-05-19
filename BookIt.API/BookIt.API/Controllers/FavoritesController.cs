using Microsoft.AspNetCore.Mvc;
using BookIt.BLL.Interfaces;
using BookIt.BLL.DTOs;
using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.API.Models.Requests;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<IEnumerable<FavoriteResponse>>> GetAllAsync()
    {
        var favoritesDto = await _service.GetAllAsync();
        var favoritesResponse = _mapper.Map<IEnumerable<FavoriteResponse>>(favoritesDto);
        return Ok(favoritesResponse);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FavoriteResponse>> GetByIdAsync([FromRoute] int id)
    {
        var favoriteDto = await _service.GetByIdAsync(id);
        var favoriteResponse = _mapper.Map<FavoriteResponse>(favoriteDto);
        return favoriteResponse is not null ? Ok(favoriteResponse) : NotFound();
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<FavoriteResponse>>> GetAllByUserIdAsync([FromRoute] int userId)
    {
        var favoritesDto = await _service.GetAllForUserAsync(userId);
        var favoritesResponse = _mapper.Map<IEnumerable<FavoriteResponse>>(favoritesDto);
        return Ok(favoritesResponse);
    }

    [HttpGet("apartment/{bookId:int}")]
    public async Task<ActionResult<int>> GetCountByBookIdAsync([FromRoute] int apartmentId)
    {
        var favoritesCount = await _service.GetCountForApartmentAsync(apartmentId);
        return Ok(favoritesCount);
    }

    [HttpPost]
    public async Task<ActionResult<FavoriteResponse>> CreateAsync([FromBody] FavoriteRequest request)
    {
        var favoriteDto = _mapper.Map<FavoriteDTO>(request);
        var added = await _service.CreateAsync(favoriteDto);
        if (added is null) return BadRequest("Failed to create favorite.");
        var favoriteResponse = _mapper.Map<FavoriteResponse>(added);
        return Ok(favoriteResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
