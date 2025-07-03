using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class ImagesRepository
{
    private readonly BookingDbContext _context;

    public ImagesRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<Image?> GetByIdAsync(int id)
    {
        return await _context.Images
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Images.AnyAsync(i => i.Id == id);
    }

    public async Task<Image> AddAsync(Image image)
    {
        await _context.Images.AddAsync(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task DeleteAsync(int id)
    {
        var image = await _context.Images
            .FirstOrDefaultAsync(i => i.Id == id);

        if (image is not null)
        {
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Image>> GetAllByApartmentIdAsync(int id)
    {
        return await _context.Images
            .Where(i => i.ApartmentId == id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Image>> GetAllByEstablishmentIdAsync(int id)
    {
        return await _context.Images
            .Where(i => i.EstablishmentId == id)
            .ToListAsync();
    }
}
