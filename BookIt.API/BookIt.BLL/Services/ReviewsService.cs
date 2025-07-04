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
    private readonly ImagesRepository _imagesRepository;
    private readonly ReviewsRepository _reviewsRepository;

    public ReviewsService(
        IMapper mapper,
        IImagesService imagesService,
        ImagesRepository imagesRepository,
        ReviewsRepository reviewsRepository)
    {
        _mapper = mapper;
        _imagesService = imagesService;
        _imagesRepository = imagesRepository;
        _reviewsRepository = reviewsRepository;
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

        await _imagesService.SaveImagesAsync(dto.Photos, BlobContainerName, setReviewIdDelegate);

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

        await _imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName);

        var photosToAdd = dto.Photos
            .Where(photo => photo.Id is null && photo.Base64Image is not null)
            .ToList();

        await _imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setReviewIdDelegate);

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reviewExists = await _reviewsRepository.ExistsAsync(id);
        if (!reviewExists) return false;

        var idsOfReviewImages = (await _imagesRepository
            .GetReviewImagesAsync(id))
            .Select(image => image.Id)
            .ToList();

        await _imagesService.DeleteImagesAsync(idsOfReviewImages, BlobContainerName);

        await _reviewsRepository.DeleteAsync(id);

        return true;
    }
}
