using System.Text.Json;
using System.Text.Json.Nodes;
using Crud.Api.Constants;
using Crud.Api.Models;
using Crud.Api.QueryModels;
using Humanizer;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crud.Api.Services
{
    public class MongoDbService : IMongoDbService
    {
        public String GetTableName(Type type)
        {
            string? tableName = type.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on type {type?.Name}.");

            return tableName;
        }

        public FilterDefinition<BsonDocument> GetIdFilter(Type type, Guid id)
        {
            FilterDefinition<BsonDocument> filter;
            if (typeof(IExternalEntity).IsAssignableFrom(type))
                filter = Builders<BsonDocument>.Filter.Eq(nameof(IExternalEntity.ExternalId), id);
            else
                filter = Builders<BsonDocument>.Filter.Eq("Id", id);

            return filter;
        }

        public FilterDefinition<BsonDocument> GetQueryParamFilter(Type type, IDictionary<String, String>? queryParams)
        {
            FilterDefinition<BsonDocument> filter = new BsonDocument();
            if (queryParams is not null)
            {
                foreach (var queryParam in queryParams)
                {
                    string key = queryParam.Key.Replace(Delimiter.QueryParamChildProperty, Delimiter.MongoDbChildProperty);
                    dynamic value = queryParam.Value.ChangeType(type.GetProperties().GetProperty(key, Delimiter.MongoDbChildProperty)!.PropertyType);
                    filter &= Builders<BsonDocument>.Filter.Eq(key.Pascalize(Delimiter.MongoDbChildProperty), value);
                }
            }

            return filter;
        }

        public IEnumerable<UpdateDefinition<BsonDocument>> GetShallowUpdates(IDictionary<String, JsonNode> propertyValues, Type type)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();

            foreach (var propertyValue in propertyValues)
            {
                string key = propertyValue.Key.Pascalize();
                dynamic value = JsonSerializer.Deserialize(propertyValue.Value, type.GetProperty(key)!.PropertyType, JsonSerializerOption.Default);
                updates.Add(Builders<BsonDocument>.Update.Set(key, value));
            }

            return updates;
        }

        public IEnumerable<UpdateDefinition<BsonDocument>> GetDeepUpdates(IDictionary<String, JsonNode> propertyValues, Type type)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();

            foreach (var propertyValue in propertyValues)
            {
                string key = propertyValue.Key.Pascalize();
                updates.AddRange(GetAllPropertiesToUpdate(key, type, propertyValue.Value));
            }

            return updates;
        }

        public IEnumerable<UpdateDefinition<BsonDocument>> GetAllPropertiesToUpdate(String propertyName, Type type, JsonNode jsonNode)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();
            string currentPropertyName = propertyName.GetValueAfterLastDelimiter(Delimiter.MongoDbChildProperty);

            if (jsonNode is JsonObject)
            {
                var propertyValues = jsonNode.Deserialize<Dictionary<string, JsonNode>>();
                foreach (var propertyValue in propertyValues!)
                {
                    updates.AddRange(GetAllPropertiesToUpdate($"{propertyName}{Delimiter.MongoDbChildProperty}{propertyValue.Key.Pascalize()}", type.GetProperty(currentPropertyName)!.PropertyType, propertyValue.Value));
                }
            }
            else
            {
                dynamic? value = jsonNode.Deserialize(type.GetProperty(currentPropertyName)!.PropertyType, JsonSerializerOption.Default);
                updates.Add(Builders<BsonDocument>.Update.Set(propertyName, value));
            }

            return updates;
        }

        public FilterDefinition<BsonDocument> GetConditionFilter(Type type, Condition? condition, String? rootLogicalOperator = Operator.And)
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

                filter = GetComparisonOperatorFilter(field, condition.ComparisonOperator, value);
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
                        conditionFilters.Add(GetConditionFilter(type, condition));
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
            if (!Operator.LogicalAliasLookup.ContainsKey(logicalOperator))
                throw new KeyNotFoundException($"{nameof(GroupedCondition.LogicalOperator)} '{logicalOperator}' was not found in {Operator.LogicalAliasLookup}.");

            return Operator.LogicalAliasLookup[logicalOperator] switch
            {
                Operator.And => Builders<BsonDocument>.Filter.And(filters),
                Operator.Or => Builders<BsonDocument>.Filter.Or(filters),
                _ => throw new NotImplementedException($"{nameof(GroupedCondition.LogicalOperator)} '{logicalOperator}' is not implemented.")
            };
        }

        public FilterDefinition<BsonDocument> GetComparisonOperatorFilter(String field, String comparisonOperator, dynamic value)
        {
            if (!Operator.ComparisonAliasLookup.ContainsKey(comparisonOperator))
                throw new KeyNotFoundException($"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' was not found in {Operator.ComparisonAliasLookup}.");

            return Operator.ComparisonAliasLookup[comparisonOperator] switch
            {
                Operator.Equality => Builders<BsonDocument>.Filter.Eq(field, value),
                _ => throw new NotImplementedException($"Unable to compare {field} to {value}. {nameof(Condition.ComparisonOperator)} '{comparisonOperator}' is not implemented.")
            };
        }

        public SortDefinition<BsonDocument> GetSort(IReadOnlyCollection<Sort>? orderBy)
        {
            var sortBuilder = Builders<BsonDocument>.Sort;

            if (orderBy is null)
                return sortBuilder.ToBsonDocument();

            var sortDefinitions = new List<SortDefinition<BsonDocument>>();
            foreach (var sort in orderBy)
            {
                var field = sort.Field!.Pascalize(Delimiter.MongoDbChildProperty);
                SortDefinition<BsonDocument> sortDefinition;
                if (sort.IsDescending.HasValue && sort.IsDescending.Value)
                {
                    sortDefinition = sortBuilder.Descending(field);
                }
                else
                {
                    sortDefinition = sortBuilder.Ascending(field);
                }

                sortDefinitions.Add(sortDefinition);
            }

            return sortBuilder.Combine(sortDefinitions);
        }

        public ProjectionDefinition<BsonDocument> GetProjections(Query? query)
        {
            var projectionBuilder = Builders<BsonDocument>.Projection;

            if (query is null)
                return projectionBuilder.ToBsonDocument();

            return projectionBuilder.Combine(GetIncludesProjections(query.Includes), GetExcludesProjections(query.Excludes));
        }

        public ProjectionDefinition<BsonDocument> GetIncludesProjections(HashSet<String>? includes)
        {
            var projectionBuilder = Builders<BsonDocument>.Projection;

            if (includes is null)
                return projectionBuilder.ToBsonDocument();

            var projectionDefinitions = new List<ProjectionDefinition<BsonDocument>>();
            foreach (var include in includes)
            {
                var field = include.Pascalize(Delimiter.MongoDbChildProperty);

                projectionDefinitions.Add(projectionBuilder.Include(field));
            }

            return projectionBuilder.Combine(projectionDefinitions);
        }

        public ProjectionDefinition<BsonDocument> GetExcludesProjections(HashSet<String>? excludes)
        {
            var projectionBuilder = Builders<BsonDocument>.Projection;

            if (excludes is null)
                return projectionBuilder.ToBsonDocument();

            var projectionDefinitions = new List<ProjectionDefinition<BsonDocument>>();
            foreach (var exclude in excludes)
            {
                var field = exclude.Pascalize(Delimiter.MongoDbChildProperty);

                projectionDefinitions.Add(projectionBuilder.Exclude(field));
            }

            return projectionBuilder.Combine(projectionDefinitions);
        }
    }
}
