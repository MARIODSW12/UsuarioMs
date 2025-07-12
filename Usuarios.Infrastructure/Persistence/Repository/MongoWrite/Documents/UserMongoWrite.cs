using MongoDB.Bson.Serialization.Attributes;

namespace Usuarios.Infrastructure.Persistence.Repository.MongoWrite.Documents
{
    public class UserMongoWrite
    {
        [BsonId]
        [BsonElement("id")]
        public required string Id { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("lastName")]
        public required string LastName { get; set; }

        [BsonElement("email")]
        public required string Email { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("phone")]
        public string? Phone { get; set; }

    }
}
