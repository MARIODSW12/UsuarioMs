using MongoDB.Bson;

using Usuarios.Infrastructure.Persistence.Repository.MongoRead.Documents;

namespace Usuarios.Infrastructure.Interfaces
{
    public interface IUserReadRepository
    {
        Task AddAsync(BsonDocument User);
        Task<UserMongoRead> GetByEmailAsync(string email);
        Task<UserMongoRead> GetByIdAsync(string id);
        Task<UserMongoRead> GetByNameAsync(string name);
        Task UpdateAsync(BsonDocument user);
        Task UpdateRoleIdById(BsonDocument user);
        Task<List<BsonDocument>> GetAllAsync();

    }
}

