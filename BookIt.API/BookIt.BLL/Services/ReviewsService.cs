using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class ReviewsService : IReviewsService
{
    private const string BlobContainerName = "reviews";

    private readonly IMapper _mapper;
    private readonly IImagesService _imagesService;
    private readonly IRatingsService _ratingsService;
    private readonly ImagesRepository _imagesRepository;
    private readonly ReviewsRepository _reviewsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;

    public ReviewsService(
        IMapper mapper,
        IImagesService imagesService,
        IRatingsService ratingsService,
        ImagesRepository imagesRepository,
        ReviewsRepository reviewsRepository,
        ApartmentsRepository apartmentsRepository)
    {
        _mapper = mapper;
        _imagesService = imagesService;
        _ratingsService = ratingsService;
        _imagesRepository = imagesRepository;
        _reviewsRepository = reviewsRepository;
        _apartmentsRepository = apartmentsRepository;
    }

    public async Task<IEnumerable<ReviewDTO>> GetAllAsync()
    {
        var reviewsDomain = await _reviewsRepository.GetAllAsync();
        var reviewsDto = _mapper.Map<IEnumerable<ReviewDTO>>(reviewsDomain);
        return reviewsDto;
    }

    public async Task<ReviewDTO?> GetByIdAsync(int id)
    {
        var reviewDomain = await _reviewsRepository.GetByIdAsync(id);
        if (reviewDomain is null) return null;
        var reviewDto = _mapper.Map<ReviewDTO>(reviewDomain);
        return reviewDto;
    }

    public async Task<ReviewDTO?> CreateAsync(ReviewDTO dto)
    {
        var reviewDomain = _mapper.Map<Review>(dto);
        var addedReview = await _reviewsRepository.AddAsync(reviewDomain);

        Action<Image> setReviewIdDelegate = image => image.ReviewId = addedReview.Id;

        Task<List<ImageDTO>> saveImagesTask = _imagesService.SaveImagesAsync(dto.Photos, BlobContainerName, setReviewIdDelegate);

        if (dto.ApartmentId.HasValue)
        {
            await _ratingsService.UpdateApartmentRatingAsync(dto.ApartmentId.Value);

            var apartment = await _apartmentsRepository.GetByIdAsync(dto.ApartmentId.Value);
            if (apartment is not null)
                await _ratingsService.UpdateEstablishmentRatingAsync(apartment.EstablishmentId);
        }
        else if (dto.CustomerId.HasValue)
        {
            await _ratingsService.UpdateUserRatingAsync(dto.CustomerId.Value);
        }

        return await GetByIdAsync(addedReview.Id);
    }

    public async Task<ReviewDTO?> UpdateAsync(int id, ReviewDTO dto)
    {
        var reviewExists = await _reviewsRepository.ExistsAsync(id);
        if (!reviewExists) return null;
        var reviewDomain = _mapper.Map<Review>(dto);
        reviewDomain.Id = id;
        await _reviewsRepository.UpdateAsync(reviewDomain);

        Action<Image> setReviewIdDelegate = image => image.ReviewId = id;

        var idsOfExistingPhotosForReview = (await _imagesRepository
            .GetReviewImagesAsync(id))
            .Select(photo => photo.Id)
            .ToList();

        var idsOfPhotosToKeep = dto.Photos
            .Where(photo => photo.Id is not null && photo.Base64Image is null)
            .Select(photo => photo.Id!.Value)
            .ToList();

        var idsOfPhotosToRemove = idsOfExistingPhotosForReview
            .Where(id => !idsOfPhotosToKeep.Contains(id))
            .ToList();

        var photosToAdd = dto.Photos
            .Where(photo => photo.Id is null && photo.Base64Image is not null)
            .ToList();

        var oldReview = await _reviewsRepository.GetByIdAsync(id);

        if (dto.ApartmentId.HasValue)
        {
            await _ratingsService.UpdateApartmentRatingAsync(dto.ApartmentId.Value);

            var apartment = await _apartmentsRepository.GetByIdAsync(dto.ApartmentId.Value);
            if (apartment is not null)
                await _ratingsService.UpdateEstablishmentRatingAsync(apartment.EstablishmentId);
        }
        else if (dto.CustomerId.HasValue)
        {
            await _ratingsService.UpdateUserRatingAsync(dto.CustomerId.Value);
        }

        if (oldReview is not null)
        {
            if (oldReview.ApartmentId.HasValue && oldReview.ApartmentId != dto.ApartmentId)
            {
                await _ratingsService.UpdateApartmentRatingAsync(oldReview.ApartmentId.Value);

                var oldApartment = await _apartmentsRepository.GetByIdAsync(oldReview.ApartmentId.Value);
                if (oldApartment is not null)
                    await _ratingsService.UpdateEstablishmentRatingAsync(oldApartment.EstablishmentId);
            }

            if (oldReview.UserId.HasValue && oldReview.UserId != dto.CustomerId)
                await _ratingsService.UpdateUserRatingAsync(oldReview.UserId.Value);
        }

        Task<bool> deleteImagesTask = _imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName);
        Task<List<ImageDTO>> saveImagesTask = _imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setReviewIdDelegate);

        await Task.WhenAll(deleteImagesTask, saveImagesTask);

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reviewDomain = await _reviewsRepository.GetByIdAsync(id);
        if (reviewDomain is null) return false;

        var idsOfReviewImages = (await _imagesRepository
            .GetReviewImagesAsync(id))
            .Select(image => image.Id)
            .ToList();

        await _imagesService.DeleteImagesAsync(idsOfReviewImages, BlobContainerName);

        var apartmentId = reviewDomain.ApartmentId;
        var userId = reviewDomain.UserId;
        int? establishmentId = null;

        if (apartmentId.HasValue)
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId.Value);
            establishmentId = apartment?.EstablishmentId;
        }

        await _reviewsRepository.DeleteAsync(id);

        if (apartmentId.HasValue)
        {
            await _ratingsService.UpdateApartmentRatingAsync(apartmentId.Value);
            if (establishmentId.HasValue)
                await _ratingsService.UpdateEstablishmentRatingAsync(establishmentId.Value);
        }
        else if (userId.HasValue)
        {
            await _ratingsService.UpdateUserRatingAsync(userId.Value);
        }

        return true;
    }
}
