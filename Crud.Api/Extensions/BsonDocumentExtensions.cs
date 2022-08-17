using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Crud.Api
{
    public static class BsonDocumentExtensions
    {
        public static T FromBsonDocument<T>(this BsonDocument bsonDocument)
        {
            return bsonDocument is null ? default : BsonSerializer.Deserialize<T>(bsonDocument);
        }
    }
}
