using Crud.Api.QueryModels;
using Crud.Api.Services;
using Crud.Api.Tests.TestingModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crud.Api.Tests.Services
{
    public class MongoDbServiceTests
    {
        private MongoDbService _mongoDbService;

        public MongoDbServiceTests()
        {
            _mongoDbService = new MongoDbService();
        }

        [Fact]
        public void GetConditionsFilter_ConditionIsNull_ReturnsEmptyFilterDefinition()
        {
            var type = typeof(Model);
            Condition? condition = null;

            var result = _mongoDbService.GetConditionsFilter(type, condition);

            Assert.NotNull(result);
            Assert.IsType(typeof(BsonDocumentFilterDefinition<BsonDocument>), result);
        }

        [Fact]
        public void GetConditionsFilter_SingleCondition_ReturnsSimpleFilterDefinition()
        {
            var type = typeof(Model);
            Condition? condition = new Condition
            {
                Field = nameof(Model.Id),
                ComparisonOperator = Operator.Equality,
                Value = "1"
            };

            var result = _mongoDbService.GetConditionsFilter(type, condition);

            Assert.NotNull(result);
            var bsonDocument = result.ToBsonDocument();
            var temp = bsonDocument.GetValue(0);
        }
    }
}
