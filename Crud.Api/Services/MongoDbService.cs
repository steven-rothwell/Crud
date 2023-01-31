using Crud.Api.Constants;
using Crud.Api.QueryModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crud.Api.Services
{
    public class MongoDbService : IMongoDbService
    {
        public FilterDefinition<BsonDocument> GetConditionsFilter(Type type, Condition? condition, String? rootLogicalOperator = Operator.And)
        {
            FilterDefinition<BsonDocument> filter = new BsonDocument();

            if (condition is null)
                return filter;

            if (condition.GroupedConditions is null)
            {
                if (condition.Field is null || condition.ComparisonOperator is null)
                    return filter;

                string field = condition.Field!.Pascalize(Delimiter.MongoDbChildProperty);
                dynamic value = condition.Value!.ChangeType(type.GetProperties().GetProperty(condition.Field, Delimiter.MongoDbChildProperty)!.PropertyType);

                filter = condition.ComparisonOperator switch
                {
                    Operator.Equality => Builders<BsonDocument>.Filter.Eq(field, value),
                    _ => throw new NotImplementedException($"Unable to compare {field} to {value}. {nameof(Condition.ComparisonOperator)} '{condition.ComparisonOperator}' is not implemented.")
                };
            }
            else
            {
                var filters = GetConditionsFilters(type, condition.GroupedConditions);
                filter = GetLogicalOperatorFilter(rootLogicalOperator ?? Operator.And, filters);
            }

            return filter;
        }

        public IEnumerable<FilterDefinition<BsonDocument>> GetConditionsFilters(Type type, IReadOnlyCollection<GroupedCondition>? groupedConditions)
        {
            if (groupedConditions is null)
                return new List<FilterDefinition<BsonDocument>> { new BsonDocument() };

            var groupedConditionFilters = new List<FilterDefinition<BsonDocument>>();
            foreach (var groupedCondition in groupedConditions)
            {
                if (groupedCondition is null || groupedCondition.LogicalOperator is null || groupedCondition.Conditions is null)
                    continue;

                var conditionFilters = new List<FilterDefinition<BsonDocument>>();
                foreach (var condition in groupedCondition.Conditions)
                {
                    if (condition.GroupedConditions is null)
                    {
                        conditionFilters.Add(GetConditionsFilter(type, condition));
                    }
                    else
                    {
                        conditionFilters.AddRange(GetConditionsFilters(type, condition.GroupedConditions));
                    }
                }

                groupedConditionFilters.Add(GetLogicalOperatorFilter(groupedCondition.LogicalOperator, conditionFilters));
            }

            return groupedConditionFilters;
        }

        public FilterDefinition<BsonDocument> GetLogicalOperatorFilter(String logicalOperator, IEnumerable<FilterDefinition<BsonDocument>> filters)
        {
            return logicalOperator switch
            {
                Operator.And => Builders<BsonDocument>.Filter.And(filters),
                Operator.Or => Builders<BsonDocument>.Filter.Or(filters),
                _ => throw new NotImplementedException($"{nameof(GroupedCondition.LogicalOperator)} '{logicalOperator}' is not implemented.")
            };
        }
    }
}
