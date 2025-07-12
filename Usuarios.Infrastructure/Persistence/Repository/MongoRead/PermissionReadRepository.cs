using MongoDB.Bson;
using MongoDB.Driver;
using log4net;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Persistence.Repository.MongoRead
{
    public class PermissionReadRepository : IPermissionReadRepository
    {
        private readonly IMongoCollection<BsonDocument> _permissionsCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PermissionReadRepository));

        public PermissionReadRepository(MongoReadDbConfig mongoConfig)
        {
            _permissionsCollection = mongoConfig.db.GetCollection<BsonDocument>("Permissions");
        }

        #region GetAllAsync
        public async Task<List<BsonDocument>> GetAllAsync()
        {
            _logger.Info("Iniciando consulta para obtener todos los permisos.");

            try
            {
                var permissions = await _permissionsCollection.Find(_ => true).ToListAsync();

                if (permissions == null || !permissions.Any())
                {
                    _logger.Warn("No se encontraron permisos en la base de datos.");
                    return new List<BsonDocument>();
                }

                _logger.Info($"Consulta completada exitosamente. Total permisos: {permissions.Count}");
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.Error("Error al obtener la lista de permisos.", ex);
                throw;
            }
        }
        #endregion

    }
}
