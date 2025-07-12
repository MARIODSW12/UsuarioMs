using MongoDB.Bson;
using MongoDB.Driver;
using log4net;

using Usuarios.Application.DTOs;

using Usuarios.Domain.Entities;
using Usuarios.Domain.ValueObjects;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Persistence.Repository
{
    public class RoleReadRepository : IRoleReadRepository
    {
        private readonly IMongoCollection<BsonDocument> _rolesCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RoleReadRepository));

        public RoleReadRepository(MongoReadDbConfig mongoConfig)
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

        #region GetAllAsync
        public async Task<List<RoleDto>> GetAllAsync()
        {
            _logger.Info("Iniciando consulta para obtener todos los roles.");

            try
            {
                var bsonRoles = await _rolesCollection.Find(_ => true).ToListAsync();

                if (bsonRoles == null || !bsonRoles.Any())
                {
                    _logger.Warn("No se encontraron roles en la base de datos.");
                    return new List<RoleDto>();
                }

                var roles = bsonRoles.Select(r => new RoleDto
                {
                    Id = r["_id"].AsString,
                    Name = r["RoleName"].AsString,
                    Permissions = r["PermissionIds"].AsBsonArray.Select(p => p.AsString).ToList()
                }).ToList();

                _logger.Info($"Consulta completada exitosamente. Total roles: {roles.Count}");
                return roles;
            }
            catch (Exception ex)
            {
                _logger.Error("Error al obtener la lista de roles.", ex);
                throw;
            }
        }
        #endregion

        #region AddPermissionsAsync(BsonDocument role)
        public async Task<bool> AddPermissionsAsync(BsonDocument role)
        {
            _logger.Info($"Iniciando actualización de permisos para el rol con ID {role["_id"]}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", role["_id"]);
                var update = Builders<BsonDocument>.Update.Push("PermissionIds", role["PermissionIds"]);

                var result = await _rolesCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    _logger.Warn($"No se modificaron permisos para el rol con ID {role["_id"]}");
                    return false;
                }

                _logger.Info($"Permisos actualizados exitosamente para el rol con ID {role["_id"]}. Documentos modificados: {result.ModifiedCount}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al actualizar permisos para el rol con ID {role["_id"]}", ex);
                throw;
            }
        }
        #endregion

        #region RemovePermissionByIdAsync(string roleId, string permissionId)
        public async Task RemovePermissionByIdAsync(string roleId, string permissionId)
        {
            _logger.Info($"Iniciando eliminación de permiso {permissionId} del rol {roleId}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", roleId);
                var update = Builders<BsonDocument>.Update.Pull("PermissionIds", permissionId);
                var result = await _rolesCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    _logger.Warn($"No se encontró el rol {roleId} o el permiso {permissionId} no estaba asignado.");
                    return;
                }

                _logger.Info($"Permiso {permissionId} eliminado exitosamente del rol {roleId}. Documentos modificados: {result.ModifiedCount}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al eliminar permiso {permissionId} del rol {roleId}", ex);
                throw;
            }
        }
        #endregion

        #region GetPermissionsByRoleIdAsync(string roleId)
        public async Task<List<string>?> GetPermissionsByRoleIdAsync(string roleId)
        {
            _logger.Info($"Buscando permisos para el rol con ID {roleId}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", roleId);
                var bsonRole = await _rolesCollection.Find(filter).FirstOrDefaultAsync();

                if (bsonRole == null)
                {
                    _logger.Warn($"No se encontró rol con ID {roleId}");
                    return null;
                }

                var permissions = bsonRole.Contains("PermissionIds")
                    ? bsonRole["PermissionIds"].AsBsonArray.Select(p => p.AsString).ToList()
                    : new List<string>();

                _logger.Info($"Permisos del rol {roleId}: {string.Join(", ", permissions)}");
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al obtener permisos del rol con ID {roleId}", ex);
                throw;
            }
        }
        #endregion

    }
}
