using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Domain.Aggregates;
using Usuarios.Domain.Repositories;
using Usuarios.Infrastructure.Configurations;

namespace Usuarios.Infrastructure.Persistence.Repository.MongoWrite
{
    public class MongoWriteUserActivityRepository
    {
        //    private readonly IMongoCollection<BsonDocument> _usersCollection;
        //    private readonly IRoleRepository _roleRepository; // Agregado para obtener el objeto Role

        //    //public MongoWriteUserActivityRepository(MongoWriteDbConfig mongoConfig, IRoleRepository roleRepository)
        //    //{
        //    //    _usersCollection = mongoConfig.db.GetCollection<BsonDocument>("usuarios_write");
        //    //    _roleRepository = roleRepository; // Inicialización del repositorio de roles
        //    //}

        //    //public async Task AddAsync(User user)
        //    //{
        //    //    var bsonUser = new BsonDocument
        //    //    {
        //    //        { "_id", user.Id.Value }, // ID como String manejado por el sistema
        //    //        { "name", user.Name.Value },
        //    //        { "lastName", user.LastName.Value },
        //    //        { "email", user.Email.Value },
        //    //        { "address", user.Address?.Value ?? "" },
        //    //        { "phone", user.Phone?.Value ?? "" },
        //    //        { "roleId", user.RoleId.Value }, // Se almacena solo el ID del rol
        //    //        //{ "isActive", true },
        //    //        { "createdAt", DateTime.UtcNow },
        //    //        { "updatedAt", DateTime.UtcNow }
        //    //    };
        //    //}
    }
}
