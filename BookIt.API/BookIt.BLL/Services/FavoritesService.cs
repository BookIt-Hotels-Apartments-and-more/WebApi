using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class FavoritesService : IFavoritesService
{
    private readonly IMapper _mapper;
    private readonly FavoritesRepository _repository;

    public FavoritesService(IMapper mapper, FavoritesRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<IEnumerable<FavoriteDTO>> GetAllAsync()
    {
        var favoritesDomain = await _repository.GetAllAsync();
        var favoritesDto = _mapper.Map<IEnumerable<FavoriteDTO>>(favoritesDomain);
        return favoritesDto;
    }

    public async Task<FavoriteDTO?> GetByIdAsync(int id)
    {
        var favoriteDomain = await _repository.GetByIdAsync(id);
        if (favoriteDomain is null) return null;
        var favoriteDto = _mapper.Map<FavoriteDTO>(favoriteDomain);
        return favoriteDto;
    }

    public async Task<IEnumerable<FavoriteDTO>> GetAllForUserAsync(int userId)
    {
        var favoritesDomain = await _repository.GetAllForUserAsync(userId);
        var favoritesDto = _mapper.Map<IEnumerable<FavoriteDTO>>(favoritesDomain);
        return favoritesDto;
    }

    public async Task<int> GetCountForApartmentAsync(int apartmentId)
    {
        var count = await _repository.GetCountForApartmentAsync(apartmentId);
        return count;
    }

    public async Task<FavoriteDTO?> CreateAsync(FavoriteDTO dto)
    {
        var favoriteDomain = _mapper.Map<Favorite>(dto);
        var addedFavorite = await _repository.AddAsync(favoriteDomain);
        return await GetByIdAsync(addedFavorite.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var favoriteExists = await _repository.ExistsAsync(id);
        if (!favoriteExists) return false;
        await _repository.DeleteAsync(id);
        return true;
    }
}
