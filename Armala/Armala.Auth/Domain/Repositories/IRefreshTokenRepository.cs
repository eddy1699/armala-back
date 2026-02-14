using Armala.Auth.Infrastructure.Persistence.Entities;

namespace Armala.Auth.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId);
    Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(Guid userId);
    Task<RefreshToken> AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllByUserIdAsync(Guid userId);
    Task DeleteExpiredTokensAsync();
}
