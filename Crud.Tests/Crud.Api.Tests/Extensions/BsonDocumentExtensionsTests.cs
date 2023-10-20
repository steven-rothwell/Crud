using Crud.Api.Tests.TestingModels;
using MongoDB.Bson;

namespace Crud.Api.Tests.Extensions
{
    public class BsonDocumentExtensionsTests
    {
        [Fact]
        public void FromBsonDocument_BsonDocumentIsNull_ReturnsDefault()
        {
            BsonDocument? bsonDocument = null;

            var result = bsonDocument.FromBsonDocument<Model>();

            Assert.Equal((Model?)default, result);
        }

        [Fact]
        public void FromBsonDocument_BsonDocumentIsNotNull_ReturnsDefault()
        {
            var model = new Model { Id = 1 };
            BsonDocument bsonDocument = model.ToBsonDocument();

            var result = bsonDocument.FromBsonDocument<Model>();

            Assert.Equal(model.Id, result!.Id);
        }
    }
}
