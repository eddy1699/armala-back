using Armala.Auth.Domain.Repositories;
using Armala.Auth.Infrastructure.Persistence.Context;
using Armala.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Armala.Armala.Auth.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ArmalaDbContext _context;

    public RefreshTokenRepository(ArmalaDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId && 
                         rt.IsRevoked == false && 
                         rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task<RefreshToken> AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllByUserIdAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsRevoked == false)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}
