using MongoDB.Bson;

namespace Usuarios.Infrastructure.Interfaces
{
    public interface IUserActivityReadRepository
    {
        Task AddAsync(BsonDocument userActivity);
        Task<List<BsonDocument>> GetAllAsync();
        Task<List<BsonDocument>> GetByUserIdAsync(string userId, DateTime timestamp);
    }
}