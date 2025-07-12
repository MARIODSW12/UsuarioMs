using MongoDB.Bson;
using MongoDB.Driver;
using log4net;

using Usuarios.Domain.Repositories;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Persistence.Repository.MongoRead
{
    public class MongoReadUserActivityRepository : IUserActivityReadRepository
    {
        private readonly IMongoCollection<BsonDocument> _usersActivityCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoReadUserActivityRepository));

        public MongoReadUserActivityRepository(MongoReadUserActivityDbConfig mongoConfig, IRoleRepository roleRepository)
        {
            _usersActivityCollection = mongoConfig.db.GetCollection<BsonDocument>("Historico");
        }

        #region AddAsync(BsonDocument userActivity)
        public async Task AddAsync(BsonDocument userActivity)
        {
            _logger.Info($"Iniciando inserción de actividad de usuario con ID {userActivity["_userid"]}");

            try
            {
                await _usersActivityCollection.InsertOneAsync(userActivity);
                _logger.Info($"Actividad de usuario con ID {userActivity["_userid"]} insertada exitosamente en la base de datos.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al insertar actividad de usuario con ID {userActivity["_userid"]} en la base de datos.", ex);
                throw;
            }
        }
        #endregion

        #region GetAllAsync
        public async Task<List<BsonDocument>> GetAllAsync()
        {
            _logger.Info("Iniciando consulta para obtener todas las actividades de usuario.");

            try
            {
                var bsonUserActivity = await _usersActivityCollection.Find(new BsonDocument()).ToListAsync();

                if (bsonUserActivity == null || !bsonUserActivity.Any())
                {
                    _logger.Warn("No se encontraron actividades de usuario en la base de datos.");
                    return new List<BsonDocument>();
                }

                _logger.Info($"Consulta completada exitosamente. Total actividades: {bsonUserActivity.Count}");
                return bsonUserActivity;
            }
            catch (Exception ex)
            {
                _logger.Error("Error al obtener actividades de usuario.", ex);
                throw;
            }
        }
        #endregion

        #region GetByUserIdAsync(string userId, DateTime timestamp)
        public async Task<List<BsonDocument>> GetByUserIdAsync(string userId, DateTime timestamp)
        {
            _logger.Info($"Iniciando consulta de actividades para el usuario {userId} desde {timestamp}");

            try
            {
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("_userid", userId),
                    Builders<BsonDocument>.Filter.Gte("timestamp", timestamp)
                );

                var bsonUserActivity = await _usersActivityCollection.Find(filter).ToListAsync();

                if (bsonUserActivity == null || !bsonUserActivity.Any())
                {
                    _logger.Warn($"No se encontraron actividades para el usuario {userId} desde {timestamp}");
                    return new List<BsonDocument>();
                }

                _logger.Info($"Consulta completada exitosamente. Total actividades encontradas: {bsonUserActivity.Count}");
                return bsonUserActivity;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al obtener actividades para el usuario {userId} desde {timestamp}", ex);
                throw;
            }
        }
        #endregion

    }
}
