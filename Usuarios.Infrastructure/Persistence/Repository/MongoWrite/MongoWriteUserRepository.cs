using MongoDB.Bson;
using MongoDB.Driver;
using log4net;

using Usuarios.Domain.Aggregates;
using Usuarios.Domain.Repositories;
using Usuarios.Domain.Factories;
using Usuarios.Domain.ValueObjects;

using Usuarios.Infrastructure.Configurations;

namespace Usuarios.Infrastructure.Persistence.Repository.MongoWrite
{
    public class MongoWriteUserRepository : IUserRepository
    {
        private readonly IMongoCollection<BsonDocument> _usersCollection;
        private readonly IRoleRepository _roleRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoWriteUserRepository));

        public MongoWriteUserRepository(MongoWriteDbConfig mongoConfig, IRoleRepository roleRepository)
        {
            _usersCollection = mongoConfig.db.GetCollection<BsonDocument>("usuarios_write");
            _roleRepository = roleRepository;

        }

        #region AddAsync(User user)
        public async Task AddAsync(User user)
        {
            _logger.Info($"Iniciando inserción de usuario con ID {user.Id.Value}");

            try
            {
                var bsonUser = new BsonDocument
                {
                    { "_id", user.Id.Value },
                    { "name", user.Name.Value },
                    { "lastName", user.LastName.Value },
                    { "email", user.Email.Value },
                    { "address", user.Address?.Value ?? "" },
                    { "phone", user.Phone?.Value ?? "" },
                    { "roleId", user.RoleId.Value },
                    { "createdAt", DateTime.UtcNow },
                    { "updatedAt", DateTime.UtcNow }
                };

                await _usersCollection.InsertOneAsync(bsonUser);
                _logger.Info($"Usuario con ID {user.Id.Value} insertado exitosamente en la base de datos.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al insertar usuario con ID {user.Id.Value} en la base de datos.", ex);
                throw;
            }
        }
        #endregion

        #region GetByEmailAsync(string email)
        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.Info($"Iniciando búsqueda de usuario con email {email}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("email", email);
                var bsonUser = await _usersCollection.Find(filter).FirstOrDefaultAsync();

                if (bsonUser == null)
                {
                    _logger.Warn($"No se encontró usuario con email {email}");
                    return null;
                }

                // Obtener el objeto Role desde MongoDB usando el RoleRepository
                var roleId = new VORoleId(bsonUser["roleId"].AsString);

                var user = UserFactory.Create(
                    new VOId(bsonUser["_id"].AsString),
                    new VOName(bsonUser["name"].AsString),
                    new VOLastName(bsonUser["lastName"].AsString),
                    new VOEmail(bsonUser["email"].AsString),
                    roleId,
                    new VOAddress(bsonUser["address"].AsString),
                    new VOPhone(bsonUser["phone"].AsString)
                );

                _logger.Info($"Usuario con email {email} encontrado exitosamente.");
                return user;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al obtener usuario con email {email}", ex);
                throw;
            }
        }
        #endregion

        #region GetByIdAsync(string id)
        public async Task<User?> GetByIdAsync(string id)
        {
            _logger.Info($"Iniciando búsqueda de usuario con ID {id}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var result = await _usersCollection.Find(filter).FirstOrDefaultAsync();

                if (result == null)
                {
                    _logger.Warn($"No se encontró usuario con ID {id}");
                    return null;
                }

                var user = new User(
                    new VOId(result["_id"].AsString),
                    new VOName(result["name"].AsString),
                    new VOLastName(result["lastName"].AsString),
                    new VOEmail(result["email"].AsString),
                    new VORoleId(result["roleId"].AsString),
                    new VOAddress(result["address"].AsString),
                    new VOPhone(result["phone"].AsString)
                );

                _logger.Info($"Usuario con ID {id} obtenido exitosamente.");
                return user;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al obtener usuario con ID {id}", ex);
                throw;
            }
        }
        #endregion

        #region UpdateAsync(User user)
        public async Task UpdateAsync(User user)
        {
            _logger.Info($"Iniciando actualización de usuario con ID {user.Id.Value}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", user.Id.Value);
                var update = Builders<BsonDocument>.Update
                    .Set("name", user.Name.Value)
                    .Set("lastName", user.LastName.Value)
                    .Set("address", user.Address.Value)
                    .Set("phone", user.Phone.Value)
                    .Set("roleId", user.RoleId.Value)
                    .Set("updatedAt", DateTime.UtcNow);

                var result = await _usersCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    _logger.Warn($"No se modificó ningún documento con ID {user.Id.Value}");
                    return;
                }

                _logger.Info($"Usuario con ID {user.Id.Value} actualizado exitosamente. Documentos modificados: {result.ModifiedCount}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al actualizar usuario con ID {user.Id.Value}", ex);
                throw;
            }
        }
        #endregion
    }
}