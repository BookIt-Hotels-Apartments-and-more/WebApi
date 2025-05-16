using BookIt.DAL.Models;
using BookIt.DAL.Database;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Models.Requests;
using BookIt.BLL.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BookIt.BLL.Services;

public class EstablishmentsService : IEstablishmentsService
{
    private readonly BookingDbContext _context;

    public EstablishmentsService(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EstablishmentResponse>> GetAllAsync()
    {
        return await _context.Establishments
            .Include(e => e.Owner)
            .Include(e => e.Photos)
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
            })
            .ToListAsync();
    }

    public async Task<EstablishmentResponse?> GetByIdAsync(int id)
    {
        var establishment = await _context.Establishments
            .Include(e => e.Owner)
            .Include(e => e.Photos)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (establishment == null) return null;
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

    public async Task<EstablishmentResponse> CreateAsync(EstablishmentRequest request)
    {
        var establishment = new Establishment
        {
            Name = request.Name,
            Address = request.Address,
            Description = request.Description,
            Rating = request.Rating,
            OwnerId = request.OwnerId
        };
        _context.Establishments.Add(establishment);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(establishment.Id);
    }

    public async Task<bool> UpdateAsync(int id, EstablishmentRequest request)
    {
        var establishment = await _context.Establishments.FindAsync(id);
        if (establishment == null) return false;
        establishment.Name = request.Name;
        establishment.Address = request.Address;
        establishment.Description = request.Description;
        establishment.Rating = request.Rating;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var establishment = await _context.Establishments.FindAsync(id);
        if (establishment == null) return false;
        _context.Establishments.Remove(establishment);
        await _context.SaveChangesAsync();
        return true;
    }
}
