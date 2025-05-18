using BookIt.DAL.Models;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Models.Requests;
using BookIt.BLL.Models.Responses;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class EstablishmentsService : IEstablishmentsService
{
    private readonly EstablishmentsRepository _repository;

    public EstablishmentsService(EstablishmentsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EstablishmentResponse>> GetAllAsync()
    {
        var establishments = await _repository.GetAllAsync();

        return establishments
            .Select(e => new EstablishmentResponse
            {
                Id = e.Id,
                Name = e.Name,
                Address = e.Address,
                Description = e.Description,
                Rating = e.Rating,
                CreatedAt = e.CreatedAt,
                Owner = new OwnerResponse
                {
                    Username = e.Owner.Username,
                    Email = e.Owner.Email,
                    Phone = e.Owner.PhoneNumber,
                    Bio = e.Owner.Bio,
                    Rating = e.Owner.Rating
                },
                Photos = e.Photos.Select(p => p.BlobUrl).ToList()
            });
    }

    public async Task<EstablishmentResponse?> GetByIdAsync(int id)
    {
        var establishment = await _repository.GetByIdAsync(id);
        if (establishment is null) return null;
        return new EstablishmentResponse
        {
            Id = establishment.Id,
            Name = establishment.Name,
            Address = establishment.Address,
            Description = establishment.Description,
            Rating = establishment.Rating,
            CreatedAt = establishment.CreatedAt,
            Owner = new OwnerResponse
            {
                Username = establishment.Owner.Username,
                Email = establishment.Owner.Email,
                Phone = establishment.Owner.PhoneNumber,
                Bio = establishment.Owner.Bio,
                Rating = establishment.Owner.Rating
            },
            Photos = establishment.Photos.Select(p => p.BlobUrl).ToList()
        };
    }

    public async Task<EstablishmentResponse?> CreateAsync(EstablishmentRequest request)
    {
        var establishment = new Establishment
        {
            Name = request.Name,
            Address = request.Address,
            Description = request.Description,
            Rating = request.Rating,
            OwnerId = request.OwnerId
        };
        var addedEstablishment = await _repository.AddAsync(establishment);
        return await GetByIdAsync(addedEstablishment.Id);
    }

    public async Task<bool> UpdateAsync(int id, EstablishmentRequest request)
    {
        var establishmentExists = await _repository.ExistsAsync(id);
        if (!establishmentExists) return false;
        await _repository.UpdateAsync(new Establishment
        {
            Id = id,
            Name = request.Name,
            Address = request.Address,
            Description = request.Description,
            Rating = request.Rating,
            OwnerId = request.OwnerId
        });
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var establishmentExists = await _repository.ExistsAsync(id);
        if (!establishmentExists) return false;
        await _repository.DeleteAsync(id);
        return true;
    }
}
