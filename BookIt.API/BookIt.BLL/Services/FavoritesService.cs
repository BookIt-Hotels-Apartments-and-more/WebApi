using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class FavoritesService : IFavoritesService
{
    private readonly IMapper _mapper;
    private readonly FavoritesRepository _repository;
    private readonly UserRepository _userRepository;
    private readonly ApartmentsRepository _apartmentsRepository;

    public FavoritesService(
        IMapper mapper,
        FavoritesRepository repository,
        UserRepository userRepository,
        ApartmentsRepository apartmentsRepository)
    {
        _mapper = mapper;
        _repository = repository;
        _userRepository = userRepository;
        _apartmentsRepository = apartmentsRepository;
    }

    public async Task<IEnumerable<FavoriteDTO>> GetAllAsync()
    {
        try
        {
            var favoritesDomain = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<FavoriteDTO>>(favoritesDomain);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve favorites", ex);
        }
    }

    public async Task<FavoriteDTO?> GetByIdAsync(int id)
    {
        try
        {
            var favoriteDomain = await _repository.GetByIdAsync(id);
            if (favoriteDomain is null)
                throw new EntityNotFoundException("Favorite", id);

            return _mapper.Map<FavoriteDTO>(favoriteDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve favorite", ex);
        }
    }

    public async Task<IEnumerable<FavoriteDTO>> GetAllForUserAsync(int userId)
    {
        try
        {
            await ValidateUserExistsAsync(userId);

            var favoritesDomain = await _repository.GetAllForUserAsync(userId);
            return _mapper.Map<IEnumerable<FavoriteDTO>>(favoritesDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve user favorites", ex);
        }
    }

    public async Task<int> GetCountForApartmentAsync(int apartmentId)
    {
        try
        {
            await ValidateApartmentExistsAsync(apartmentId);

            return await _repository.GetCountForApartmentAsync(apartmentId);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to get apartment favorites count", ex);
        }
    }

    public async Task<FavoriteDTO?> CreateAsync(FavoriteDTO dto)
    {
        try
        {
            ValidateFavoriteData(dto);

            await ValidateUserExistsAsync(dto.UserId);
            await ValidateApartmentExistsAsync(dto.ApartmentId);

            await ValidateFavoriteDoesNotExistAsync(dto.UserId, dto.ApartmentId);

            var favoriteDomain = _mapper.Map<Favorite>(dto);
            var addedFavorite = await _repository.AddAsync(favoriteDomain);

            return await GetByIdAsync(addedFavorite.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to create favorite", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var favoriteExists = await _repository.ExistsAsync(id);
            if (!favoriteExists)
                throw new EntityNotFoundException("Favorite", id);

            await _repository.DeleteAsync(id);
            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to delete favorite", ex);
        }
    }

    private void ValidateFavoriteData(FavoriteDTO dto)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (dto.UserId <= 0) validationErrors.Add("UserId", new List<string> { "Valid user ID is required" });
        if (dto.ApartmentId <= 0) validationErrors.Add("ApartmentId", new List<string> { "Valid apartment ID is required" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private async Task ValidateUserExistsAsync(int userId)
    {
        if (!await _userRepository.ExistsByIdAsync(userId))
            throw new EntityNotFoundException("User", userId);
    }

    private async Task ValidateApartmentExistsAsync(int apartmentId)
    {
        if (!await _apartmentsRepository.ExistsAsync(apartmentId))
            throw new EntityNotFoundException("Apartment", apartmentId);
    }

    private async Task ValidateFavoriteDoesNotExistAsync(int userId, int apartmentId)
    {
        if (await _repository.GetByUserAndApartmentAsync(userId, apartmentId) is not null)
            throw new EntityAlreadyExistsException("Favorite", "user and apartment combination", $"User {userId} - Apartment {apartmentId}");
    }
}