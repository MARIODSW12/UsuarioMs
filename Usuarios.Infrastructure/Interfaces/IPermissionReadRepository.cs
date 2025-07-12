using MongoDB.Bson;

namespace Usuarios.Infrastructure.Interfaces
{
    public interface IPermissionReadRepository
    {
        Task<List<BsonDocument>> GetAllAsync();
    }
}
