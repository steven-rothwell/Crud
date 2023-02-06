using System.Text.Json.Nodes;
using Crud.Api.QueryModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crud.Api.Services
{
    public interface IMongoDbService
    {
        String GetTableName(Type type);
        FilterDefinition<BsonDocument> GetIdFilter(Type type, Guid id);
        FilterDefinition<BsonDocument> GetQueryParamFilter(Type type, IDictionary<String, String>? queryParams);
        IEnumerable<UpdateDefinition<BsonDocument>> GetShallowUpdates(IDictionary<String, JsonNode> propertyValues, Type type);
        IEnumerable<UpdateDefinition<BsonDocument>> GetDeepUpdates(IDictionary<String, JsonNode> propertyValues, Type type);
        IEnumerable<UpdateDefinition<BsonDocument>> GetAllPropertiesToUpdate(String propertyName, Type type, JsonNode jsonNode);
        FilterDefinition<BsonDocument> GetConditionFilter(Type type, Condition? condition, String? rootLogicalOperator = Operator.And);
        IEnumerable<FilterDefinition<BsonDocument>> GetConditionsFilters(Type type, IReadOnlyCollection<GroupedCondition>? groupedConditions);
        FilterDefinition<BsonDocument> GetLogicalOperatorFilter(String logicalOperator, IEnumerable<FilterDefinition<BsonDocument>> filters);
        FilterDefinition<BsonDocument> GetComparisonOperatorFilter(String field, String comparisonOperator, dynamic value);
    }
}
