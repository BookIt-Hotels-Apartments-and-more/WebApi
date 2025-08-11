using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BookIt.BLL.Services;

public class FavoritesService : IFavoritesService
{
    private readonly IMapper _mapper;
    private readonly UserRepository _userRepository;
    private readonly FavoritesRepository _repository;
    private readonly ILogger<FavoritesService> _logger;
    private readonly ApartmentsRepository _apartmentsRepository;

    public FavoritesService(
        IMapper mapper,
        UserRepository userRepository,
        FavoritesRepository repository,
        ILogger<FavoritesService> logger,
        ApartmentsRepository apartmentsRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _repository = repository;
        _userRepository = userRepository;
        _apartmentsRepository = apartmentsRepository;
    }

    public async Task<IEnumerable<FavoriteDTO>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all favorites");
        try
        {
            var favoritesDomain = await _repository.GetAllAsync();
            var result = _mapper.Map<IEnumerable<FavoriteDTO>>(favoritesDomain);
            _logger.LogInformation("Retrieved {Count} favorites", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve favorites");
            throw new ExternalServiceException("Database", "Failed to retrieve favorites", ex);
        }
    }

    public async Task<FavoriteDTO?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Start GetByIdAsync for Favorite Id: {Id}", id);
        try
        {
            var favoriteDomain = await _repository.GetByIdAsync(id);
            if (favoriteDomain is null)
            {
                _logger.LogWarning("Favorite with Id {Id} not found", id);
                throw new EntityNotFoundException("Favorite", id);
            }

            var result = _mapper.Map<FavoriteDTO>(favoriteDomain);
            _logger.LogInformation("Retrieved favorite with Id {Id}", id);
            return result;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve favorite with Id {Id}", id);
            throw new ExternalServiceException("Database", "Failed to retrieve favorite", ex);
        }
    }

    public async Task<IEnumerable<FavoriteDTO>> GetAllForUserAsync(int userId)
    {
        _logger.LogInformation("Start GetAllForUserAsync for User Id: {UserId}", userId);
        try
        {
            await ValidateUserExistsAsync(userId);

            var favoritesDomain = await _repository.GetAllForUserAsync(userId);
            var result = _mapper.Map<IEnumerable<FavoriteDTO>>(favoritesDomain);
            _logger.LogInformation("Retrieved {Count} favorites for User Id {UserId}", result.Count(), userId);
            return result;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve favorites for User Id {UserId}", userId);
            throw new ExternalServiceException("Database", "Failed to retrieve user favorites", ex);
        }
    }

    public async Task<int> GetCountForApartmentAsync(int apartmentId)
    {
        _logger.LogInformation("Start GetCountForApartmentAsync for Apartment Id: {ApartmentId}", apartmentId);
        try
        {
            await ValidateApartmentExistsAsync(apartmentId);

            var count = await _repository.GetCountForApartmentAsync(apartmentId);
            _logger.LogInformation("Count for Apartment Id {ApartmentId} is {Count}", apartmentId, count);
            return count;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get apartment favorites count for Apartment Id {ApartmentId}", apartmentId);
            throw new ExternalServiceException("Database", "Failed to get apartment favorites count", ex);
        }
    }

    public async Task<FavoriteDTO?> CreateAsync(FavoriteDTO dto)
    {
        _logger.LogInformation("Start CreateAsync for Favorite UserId: {UserId}, ApartmentId: {ApartmentId}", dto.UserId, dto.ApartmentId);
        try
        {
            ValidateFavoriteData(dto);

            await ValidateUserExistsAsync(dto.UserId);
            await ValidateApartmentExistsAsync(dto.ApartmentId);
            await ValidateFavoriteDoesNotExistAsync(dto.UserId, dto.ApartmentId);

            var favoriteDomain = _mapper.Map<Favorite>(dto);
            var addedFavorite = await _repository.AddAsync(favoriteDomain);

            _logger.LogInformation("Successfully created favorite with Id {Id}", addedFavorite.Id);

            return await GetByIdAsync(addedFavorite.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create favorite UserId: {UserId}, ApartmentId: {ApartmentId}", dto.UserId, dto.ApartmentId);
            throw new ExternalServiceException("Database", "Failed to create favorite", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Start DeleteAsync for Favorite Id: {Id}", id);
        try
        {
            var favoriteExists = await _repository.ExistsAsync(id);
            if (!favoriteExists)
            {
                _logger.LogWarning("Favorite with Id {Id} not found for deletion", id);
                throw new EntityNotFoundException("Favorite", id);
            }

            await _repository.DeleteAsync(id);

            _logger.LogInformation("Successfully deleted favorite with Id {Id}", id);

            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete favorite with Id {Id}", id);
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
