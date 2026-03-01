using Microsoft.EntityFrameworkCore;
using Armala.Auth.Domain.Repositories;
using Armala.Auth.Infrastructure.Persistence.Context;
using Armala.Auth.Infrastructure.Persistence.Entities;

namespace Armala.Auth.Infrastructure.Persistence.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly ArmalaDbContext _context;

    public OtpRepository(ArmalaDbContext context)
    {
        _context = context;
    }

    public async Task<Otp?> GetActiveByUserIdAndPurposeAsync(Guid userId, string purpose)
    {
        return await _context.Otps
            .AsNoTracking()
            .Where(o => o.UserId == userId
                     && o.Purpose == purpose
                     && !o.IsUsed
                     && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Otp otp)
    {
        await _context.Otps.AddAsync(otp);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Otp otp)
    {
        _context.Otps.Update(otp);
        await _context.SaveChangesAsync();
    }

    public async Task InvalidateAllByUserIdAndPurposeAsync(Guid userId, string purpose)
    {
        var otps = await _context.Otps
            .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
            .ToListAsync();

        foreach (var otp in otps)
            otp.IsUsed = true;

        await _context.SaveChangesAsync();
    }
}
