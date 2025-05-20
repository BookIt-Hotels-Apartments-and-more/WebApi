using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class ReviewsService : IReviewsService
{
    private readonly IMapper _mapper;
    private readonly ReviewsRepository _repository;

    public ReviewsService(IMapper mapper, ReviewsRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<IEnumerable<ReviewDTO>> GetAllAsync()
    {
        var reviewsDomain = await _repository.GetAllAsync();
        var reviewsDto = _mapper.Map<IEnumerable<ReviewDTO>>(reviewsDomain);
        return reviewsDto;
    }

    public async Task<ReviewDTO?> GetByIdAsync(int id)
    {
        var reviewDomain = await _repository.GetByIdAsync(id);
        if (reviewDomain is null) return null;
        var reviewDto = _mapper.Map<ReviewDTO>(reviewDomain);
        return reviewDto;
    }

    public async Task<ReviewDTO?> CreateAsync(ReviewDTO dto)
    {
        var reviewDomain = _mapper.Map<Review>(dto);
        var addedReview = await _repository.AddAsync(reviewDomain);
        return await GetByIdAsync(addedReview.Id);
    }

    public async Task<ReviewDTO?> UpdateAsync(int id, ReviewDTO dto)
    {
        var reviewExists = await _repository.ExistsAsync(id);
        if (!reviewExists) return null;
        var reviewDomain = _mapper.Map<Review>(dto);
        reviewDomain.Id = id;
        await _repository.UpdateAsync(reviewDomain);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reviewExists = await _repository.ExistsAsync(id);
        if (!reviewExists) return false;
        await _repository.DeleteAsync(id);
        return true;
    }
}
