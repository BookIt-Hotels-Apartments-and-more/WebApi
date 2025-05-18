using BookIt.BLL.Interfaces;
using BookIt.BLL.Models.Requests;
using BookIt.BLL.Models.Responses;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class ApartmentsService : IApartmentsService
{
    private readonly ApartmentsRepository _repository;

    public ApartmentsService(ApartmentsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ApartmentResponse>> GetAllAsync()
    {
        var apartments = await _repository.GetAllAsync();

        return apartments
            .Select(e => new ApartmentResponse
            {
                Id = e.Id,
                Name = e.Name,
                Price = e.Price,
                Capacity = e.Capacity,
                Rating = e.Rating,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
                Establishment = new(),
                Photos = e.Photos.Select(p => p.BlobUrl).ToList()
            });
    }

    public async Task<ApartmentResponse?> GetByIdAsync(int id)
    {
        var apartment = await _repository.GetByIdAsync(id);
        if (apartment is null) return null;
        return new ApartmentResponse
        {
            Id = apartment.Id,
            Name = apartment.Name,
            Price = apartment.Price,
            Capacity = apartment.Capacity,
            Rating = apartment.Rating,
            Description = apartment.Description,
            CreatedAt = apartment.CreatedAt,
            Establishment = new(),
            Photos = apartment.Photos.Select(p => p.BlobUrl).ToList()
        };
    }

    public async Task<ApartmentResponse?> CreateAsync(ApartmentRequest request)
    {
        var apartment = new Apartment
        {
            Name = request.Name,
            Price = request.Price,
            Capacity = request.Capacity,
            Rating = request.Rating,
            Description = request.Description,
            EstablishmentId = request.EstablishmentId
        };
        var addedApartment = await _repository.AddAsync(apartment);
        return await GetByIdAsync(addedApartment.Id);
    }

    public async Task<bool> UpdateAsync(int id, ApartmentRequest request)
    {
        var apartmentExists = await _repository.ExistsAsync(id);
        if (!apartmentExists) return false;
        await _repository.UpdateAsync(new Apartment
        {
            Id = id,
            Name = request.Name,
            Price = request.Price,
            Capacity = request.Capacity,
            Rating = request.Rating,
            Description = request.Description,
            EstablishmentId = request.EstablishmentId
        });
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var apartmentExists = await _repository.ExistsAsync(id);
        if (!apartmentExists) return false;
        await _repository.DeleteAsync(id);
        return true;
    }
}
