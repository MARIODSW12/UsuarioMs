using MongoDB.Driver;
using log4net;

namespace Usuarios.Infrastructure.Configurations
{
    public class MongoReadUserActivityDbConfig
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoReadUserActivityDbConfig));
        public MongoClient client;
        public IMongoDatabase db;

        public MongoReadUserActivityDbConfig()
        {
            try
            {
                string connectionUri = Environment.GetEnvironmentVariable("MONGODB_CNN_READ_ACTIVITY");

                if (string.IsNullOrWhiteSpace(connectionUri))
                {
                    _logger.Error("La cadena de conexión de MongoDB no está definida.");
                    throw new ArgumentException("La cadena de conexión de MongoDB no está definida.");
                }

                var settings = MongoClientSettings.FromConnectionString(connectionUri);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);

                client = new MongoClient(settings);

                string databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME_READ_ACTIVITY");
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    _logger.Error("El nombre de la base de datos de MongoDB no está definido.");
                    throw new ArgumentException("El nombre de la base de datos de MongoDB no está definido.");
                }

                db = client.GetDatabase(databaseName);
                _logger.Info("Conexión a MongoDB establecida correctamente.");
            }
            catch (MongoException ex)
            {
                _logger.Error("Error al conectar con MongoDB.", ex);
                throw new Exception("No se pudo conectar a la base de datos.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al establecer la conexión con MongoDB.", ex);
                throw;
            }
        }
    }
}
