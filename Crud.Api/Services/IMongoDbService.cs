using Crud.Api.QueryModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crud.Api.Services
{
    public interface IMongoDbService
    {
        FilterDefinition<BsonDocument> GetConditionsFilter(Type type, Condition? condition, String? rootLogicalOperator = Operator.And);
        IEnumerable<FilterDefinition<BsonDocument>> GetConditionsFilters(Type type, IReadOnlyCollection<GroupedCondition>? groupedConditions);
        FilterDefinition<BsonDocument> GetLogicalOperatorFilter(String logicalOperator, IEnumerable<FilterDefinition<BsonDocument>> filters);
    }
}
