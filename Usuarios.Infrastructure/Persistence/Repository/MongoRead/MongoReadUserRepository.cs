using MongoDB.Driver;
using MongoDB.Bson;
using log4net;

using Usuarios.Domain.Repositories;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Persistence.Repository.MongoRead.Documents;
using System.Text.RegularExpressions;

namespace Usuarios.Infrastructure.Persistence.Repository.MongoRead
{
    public class MongoReadUserRepository : IUserReadRepository
    {
        private readonly IMongoCollection<BsonDocument> _usersCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoReadUserRepository));

        public MongoReadUserRepository(MongoReadDbConfig mongoConfig, IRoleRepository roleRepository)
        {
            _usersCollection = mongoConfig.db.GetCollection<BsonDocument>("usuarios");
        }

        #region AddAsync(BsonDocument user)
        public async Task AddAsync(BsonDocument user)
        {
            _logger.Info($"Iniciando inserción de usuario con ID {user["_id"]}");

            try
            {
                await _usersCollection.InsertOneAsync(user);
                _logger.Info($"Usuario con ID {user["_id"]} insertado exitosamente en la base de datos.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al insertar usuario con ID {user["_id"]} en la base de datos.", ex);
                throw;
            }
        }
        #endregion

        #region GetByEmailAsync(string email)
        public async Task<UserMongoRead?> GetByEmailAsync(string email)
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

                var user = new UserMongoRead
                {
                    Id = bsonUser["_id"].AsString,
                    Name = bsonUser["name"].AsString,
                    LastName = bsonUser["lastName"].AsString,
                    Email = bsonUser["email"].AsString,
                    roleId = bsonUser["roleId"].AsString,
                    Address = bsonUser["address"].AsString,
                    Phone = bsonUser["phone"].AsString
                };

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

        #region GetById
        public async Task<UserMongoRead?> GetByIdAsync(string id)
        {

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var bsonUser = await _usersCollection.Find(filter).FirstOrDefaultAsync();

                if (bsonUser == null)
                {
                    return null;
                }

                var user = new UserMongoRead
                {
                    Id = bsonUser["_id"].AsString,
                    Name = bsonUser["name"].AsString,
                    LastName = bsonUser["lastName"].AsString,
                    Email = bsonUser["email"].AsString,
                    roleId = bsonUser["roleId"].AsString,
                    Address = bsonUser["address"].AsString,
                    Phone = bsonUser["phone"].AsString
                };

                return user;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region GetByNameAsync
        public async Task<UserMongoRead?> GetByNameAsync(string name)
        {

            try
            {;
                var filter = Builders<BsonDocument>.Filter.Regex("name", $"/{name}/i");
                var bsonUser = await _usersCollection.Find(filter).FirstOrDefaultAsync();
                if (bsonUser == null)
                {
                    return null;
                }

                var user = new UserMongoRead
                {
                    Id = bsonUser["_id"].AsString,
                    Name = bsonUser["name"].AsString,
                    LastName = bsonUser["lastName"].AsString,
                    Email = bsonUser["email"].AsString,
                    roleId = bsonUser["roleId"].AsString,
                    Address = bsonUser["address"].AsString,
                    Phone = bsonUser["phone"].AsString
                };

                return user;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region UpdateAsync(BsonDocument user)
        public async Task UpdateAsync(BsonDocument user)
        {
            _logger.Info($"Iniciando actualización de usuario con ID {user["_id"]}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", user["_id"]);
                var update = Builders<BsonDocument>.Update
                    .Set("name", user["name"])
                    .Set("lastName", user["lastName"])
                    .Set("address", user["address"])
                    .Set("phone", user["phone"])
                    .Set("updatedAt", DateTime.UtcNow);

                var result = await _usersCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    _logger.Warn($"No se modificó ningún documento con ID {user["_id"]}");
                    return;
                }

                _logger.Info($"Usuario con ID {user["_id"]} actualizado exitosamente. Documentos modificados: {result.ModifiedCount}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al actualizar usuario con ID {user["_id"]}", ex);
                throw;
            }
        }
        #endregion

        #region UpdateRoleIdById(BsonDocument user)
        public async Task UpdateRoleIdById(BsonDocument user)
        {
            _logger.Info($"Iniciando actualización de RoleId para el usuario con ID {user["_id"]}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", user["_id"]);
                var update = Builders<BsonDocument>.Update.Set("roleId", user["roleId"]);
                var result = await _usersCollection.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    _logger.Warn($"No se encontró usuario con ID {user["_id"]} para actualizar el RoleId.");
                    throw new KeyNotFoundException("No se encontró el usuario para actualizar el RoleId.");
                }

                _logger.Info($"Usuario con ID {user["_id"]} actualizado exitosamente. RoleId modificado.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al actualizar RoleId para el usuario con ID {user["_id"]}", ex);
                throw;
            }
        }
        #endregion

        #region GetAllAsync
        public async Task<List<BsonDocument>> GetAllAsync()
        {
            _logger.Info("Iniciando consulta para obtener todos los usuarios.");

            try
            {
                var users = await _usersCollection.Find(_ => true).ToListAsync();

                if (users == null || !users.Any())
                {
                    _logger.Warn("No se encontraron usuarios en la base de datos.");
                    return new List<BsonDocument>();
                }

                _logger.Info($"Consulta completada exitosamente. Total usuarios: {users.Count}");
                return users;
            }
            catch (Exception ex)
            {
                _logger.Error("Error al obtener la lista de usuarios.", ex);
                throw;
            }
        }
        #endregion
    }
}