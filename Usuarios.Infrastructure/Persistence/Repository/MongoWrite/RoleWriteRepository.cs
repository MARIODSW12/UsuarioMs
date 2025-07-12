using log4net;
using MongoDB.Bson;
using MongoDB.Driver;

using Usuarios.Domain.Entities;
using Usuarios.Domain.Repositories;
using Usuarios.Domain.ValueObjects;

using Usuarios.Infrastructure.Configurations;

namespace Usuarios.Infrastructure.Persistence.Repository.MongoWrite
{
    public class RoleWriteRepository : IRoleRepository
    {
        private readonly IMongoCollection<BsonDocument> _rolesCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RoleWriteRepository));

        public RoleWriteRepository(MongoWriteDbConfig mongoConfig)
        {
            _rolesCollection = mongoConfig.db.GetCollection<BsonDocument>("Roles");
        }

        #region GetByIdAsync(string roleId)
        public async Task<Role?> GetByIdAsync(string roleId)
        {
            _logger.Info($"Iniciando búsqueda de rol con ID {roleId}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", roleId);
                var bsonRole = await _rolesCollection.Find(filter).FirstOrDefaultAsync();

                if (bsonRole == null)
                {
                    _logger.Warn($"No se encontró rol con ID {roleId}");
                    return null;
                }

                var role = new Role(
                    new VORoleId(bsonRole["_id"].AsString),
                    new VORoleName(bsonRole["RoleName"].AsString),
                    new VORolePermissions(bsonRole["PermissionIds"].AsBsonArray.Select(p => p.AsString).ToList())
                );

                _logger.Info($"Rol con ID {roleId} obtenido exitosamente.");
                return role;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al obtener rol con ID {roleId}", ex);
                throw;
            }
        }
        #endregion

        #region UpdateAsync(Role role)
        public async Task<bool> UpdateAsync(Role role)
        {
            _logger.Info($"Iniciando actualización del rol con ID {role.Id.Value}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", role.Id.Value);
                var update = Builders<BsonDocument>.Update
                    .Set("PermissionIds", role.PermissionIds.PermissionIds)
                    .Set("updatedAt", DateTime.UtcNow);

                var result = await _rolesCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    _logger.Warn($"No se modificó ningún rol con ID {role.Id.Value}");
                    return false;
                }

                _logger.Info($"Rol con ID {role.Id.Value} actualizado exitosamente. Documentos modificados: {result.ModifiedCount}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al actualizar rol con ID {role.Id.Value}", ex);
                throw;
            }
        }
        #endregion

        #region AddPermissionsAsync(VORoleId roleId, VORolePermissions permissions)
        public async Task<bool> AddPermissionsAsync(VORoleId roleId, VORolePermissions permissions)
        {
            _logger.Info($"Iniciando actualización de permisos para el rol {roleId.Value}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", roleId.Value);
                var update = Builders<BsonDocument>.Update.Set("PermissionIds", new BsonArray(permissions.PermissionIds));

                var result = await _rolesCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    _logger.Warn($"No se modificaron permisos para el rol {roleId.Value}");
                    return false;
                }

                _logger.Info($"Permisos actualizados exitosamente para el rol {roleId.Value}. Documentos modificados: {result.ModifiedCount}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al actualizar permisos para el rol {roleId.Value}", ex);
                throw;
            }
        }
        #endregion

    }
}
