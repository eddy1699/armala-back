using Armala.Auth.Infrastructure.Persistence.Entities;

namespace Armala.Auth.Domain.Repositories;

public interface IOtpRepository
{
    Task<Otp?> GetActiveByUserIdAndPurposeAsync(Guid userId, string purpose);
    Task AddAsync(Otp otp);
    Task UpdateAsync(Otp otp);
    Task InvalidateAllByUserIdAndPurposeAsync(Guid userId, string purpose);
}
