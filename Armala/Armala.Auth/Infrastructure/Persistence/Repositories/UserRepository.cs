using Microsoft.EntityFrameworkCore;
using Armala.Auth.Domain.Repositories;
using Armala.Auth.Infrastructure.Persistence.Context;
using Armala.Auth.Infrastructure.Persistence.Entities;

namespace Armala.Auth.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ArmalaDbContext _context;

    public UserRepository(ArmalaDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<User?> GetByDniAsync(string dni)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Dni == dni);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users
            .AnyAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<bool> ExistsByDniAsync(string dni)
    {
        return await _context.Users
            .AnyAsync(u => u.Dni == dni);
    }

    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetNewUsersInWindowAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .ToListAsync();
    }

    public async Task<int> CountVerifiedUsersAsync()
    {
        return await _context.Users
            .CountAsync(u => u.IsVerifired == true);
    }
}
