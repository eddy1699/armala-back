using Armala.Auth.Infrastructure.Persistence.Entities;

namespace Armala.Auth.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByDniAsync(string dni);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
    Task<bool> ExistsByDniAsync(string dni);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task<IEnumerable<User>> GetNewUsersInWindowAsync(DateTime startDate, DateTime endDate);
    Task<int> CountVerifiedUsersAsync();
}
