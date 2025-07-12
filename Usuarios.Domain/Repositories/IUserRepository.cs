using Usuarios.Domain.Aggregates;

namespace Usuarios.Domain.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(string id);

    }
}
