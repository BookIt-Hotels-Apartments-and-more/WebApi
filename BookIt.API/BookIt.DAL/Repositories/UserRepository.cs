using BookIt.DAL.Models;
using BookIt.DAL.Database;
using Microsoft.EntityFrameworkCore;
using BookIt.DAL.Enums;

namespace BookIt.DAL.Repositories;

public class UserRepository
{
    private readonly BookingDbContext _context;

    public UserRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Photos)
            .Include(u => u.Reviews)
            .Include(u => u.Bookings)
            .Include(u => u.Favorites)
            .Include(u => u.OwnedEstablishments)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Photos)
            .Include(u => u.Reviews)
            .Include(u => u.Bookings)
            .Include(u => u.Favorites)
            .Include(u => u.OwnedEstablishments)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAndPasswordHashAsync(string email, string passwordHash)
    {
        return await _context.Users
            .Include(u => u.Photos)
            .Include(u => u.Bookings)
            .Include(u => u.Favorites)
            .Include(u => u.OwnedEstablishments)
            .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);
    }

    public async Task<User?> GetByEmailTokenAsync(string token)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
    }

    public async Task<User?> GetByResetPasswordTokenAsync(string token)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.ResetPasswordToken == token);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Photos)
            .Include(u => u.Bookings)
            .Include(u => u.Favorites)
            .Include(u => u.OwnedEstablishments)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateAsync(User updatedUser)
    {
        var user = await _context.Users.FindAsync(updatedUser.Id);
        if (user is null) return false;

        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;
        user.PasswordHash = updatedUser.PasswordHash;
        user.PhoneNumber = updatedUser.PhoneNumber;
        user.Bio = updatedUser.Bio;
        user.Role = updatedUser.Role;
        user.EmailConfirmationToken = updatedUser.EmailConfirmationToken;
        user.IsEmailConfirmed = updatedUser.IsEmailConfirmed;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateUserLastActivityAtAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null) return;

        user.LastActiveAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task SetUserRoleAsync(int id, UserRole role)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null) return;

        user.Role = role;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<User>> GetAllByRoleAsync(UserRole role)
    {
        return await _context.Users
            .Where(u => u.Role == role)
            .Include(u => u.Photos)
            .Include(u => u.Reviews)
            .Include(u => u.Bookings)
            .Include(u => u.Favorites)
            .Include(u => u.OwnedEstablishments)
            .ToListAsync();
    }
}
