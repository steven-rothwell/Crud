using System.Collections;
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
        public String GetTableName(Type? type)
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
                filter = Builders<BsonDocument>.Filter.Eq(nameof(IExternalEntity.ExternalId).Camelize(), id);
            else
                filter = Builders<BsonDocument>.Filter.Eq("id", id);

            return filter;
        }

        public FilterDefinition<BsonDocument> GetQueryParamFilter(Type type, IDictionary<String, String>? queryParams)
        {
            FilterDefinition<BsonDocument> filter = new BsonDocument();
            if (queryParams is not null)
            {
                foreach (var queryParam in queryParams)
                {
                    var propertyInfo = type.GetProperties().GetProperty(queryParam.Key, Delimiter.MongoDbChildProperty);
                    string key = propertyInfo!.Name.Replace(Delimiter.QueryParamChildProperty, Delimiter.MongoDbChildProperty);
                    dynamic? value = queryParam.Value.ChangeType(propertyInfo!.PropertyType);
                    filter &= Builders<BsonDocument>.Filter.Eq(key.Camelize(Delimiter.MongoDbChildProperty), value);
                }
            }

            return filter;
        }

        public IEnumerable<UpdateDefinition<BsonDocument>> GetShallowUpdates(IDictionary<String, JsonElement> propertyValues, Type type)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();

            foreach (var propertyValue in propertyValues)
            {
                string key = propertyValue.Key.Camelize();
                dynamic? value = JsonSerializer.Deserialize(propertyValue.Value, type.GetProperty(key.Pascalize())!.PropertyType, JsonSerializerOption.Default);

                if (value is null)
                {
                    updates.Add(Builders<BsonDocument>.Update.Set(key, BsonNull.Value));
                }
                else
                {
                    updates.Add(Builders<BsonDocument>.Update.Set(key, value));
                }
            }

            return updates;
        }

        public IEnumerable<UpdateDefinition<BsonDocument>> GetDeepUpdates(IDictionary<String, JsonElement> propertyValues, Type type)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();

            foreach (var propertyValue in propertyValues)
            {
                string key = propertyValue.Key.Camelize();
                updates.AddRange(GetAllPropertiesToUpdate(key, type, propertyValue.Value));
            }

            return updates;
        }

        public IEnumerable<UpdateDefinition<BsonDocument>> GetAllPropertiesToUpdate(String propertyName, Type type, JsonElement jsonElement)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();
            string currentPropertyName = propertyName.GetValueAfterLastDelimiter(Delimiter.MongoDbChildProperty).Pascalize();

            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                var propertyValues = jsonElement.Deserialize<Dictionary<string, JsonElement>>();
                foreach (var propertyValue in propertyValues!)
                {
                    updates.AddRange(GetAllPropertiesToUpdate($"{propertyName}{Delimiter.MongoDbChildProperty}{propertyValue.Key.Camelize()}", type.GetProperty(currentPropertyName)!.PropertyType, propertyValue.Value));
                }
            }
            else
            {
                dynamic? value = jsonElement.Deserialize(type.GetProperty(currentPropertyName)!.PropertyType, JsonSerializerOption.Default);

                if (value is null)
                {
                    updates.Add(Builders<BsonDocument>.Update.Set(propertyName, BsonNull.Value));
                }
                else
                {
                    updates.Add(Builders<BsonDocument>.Update.Set(propertyName, value));
                }
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

                var fieldPropertyInfo = type.GetProperties().GetProperty(condition.Field, Delimiter.MongoDbChildProperty);
                string field = fieldPropertyInfo!.Name.Camelize(Delimiter.MongoDbChildProperty);
                Type fieldType = fieldPropertyInfo!.PropertyType;

                if (typeof(IEnumerable).IsAssignableFrom(fieldType) && fieldType.IsGenericType)
                {
                    fieldType = fieldType.GenericTypeArguments.First();
                }

                if (condition.Values is not null)
                {
                    //IEnumerable<dynamic> values = condition.Values!.Select(value => ChangeType(field, fieldType, value));
                    dynamic values = condition.Values!.Select(value => ChangeType(field, fieldType, value)).ToList();
                    filter = GetComparisonOperatorFilterValues(field, condition.ComparisonOperator, values);
                }
                else
                {
                    dynamic value = ChangeType(field, fieldType, condition.Value!);
                    filter = GetComparisonOperatorFilter(field, condition.ComparisonOperator, value);
                }
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
                Operator.Inequality => Builders<BsonDocument>.Filter.Ne(field, value),
                Operator.GreaterThan => Builders<BsonDocument>.Filter.Gt(field, value),
                Operator.GreaterThanOrEquals => Builders<BsonDocument>.Filter.Gte(field, value),
                Operator.LessThan => Builders<BsonDocument>.Filter.Lt(field, value),
                Operator.LessThanOrEquals => Builders<BsonDocument>.Filter.Lte(field, value),
                Operator.Contains => Builders<BsonDocument>.Filter.Regex(field, new BsonRegularExpression(value, "i")),
                Operator.StartsWith => Builders<BsonDocument>.Filter.Regex(field, new BsonRegularExpression($"^{value}", "i")),
                Operator.EndsWith => Builders<BsonDocument>.Filter.Regex(field, new BsonRegularExpression($"{value}$", "i")),
                _ => throw new NotImplementedException($"Unable to compare {field} to {value}. {nameof(Condition.ComparisonOperator)} '{comparisonOperator}' is not implemented.")
            };
        }

        //public FilterDefinition<BsonDocument> GetComparisonOperatorFilter(String field, String comparisonOperator, IEnumerable<dynamic> values)
        public FilterDefinition<BsonDocument> GetComparisonOperatorFilterValues(String field, String comparisonOperator, dynamic values)
        {
            if (!Operator.ComparisonAliasLookup.ContainsKey(comparisonOperator))
                throw new KeyNotFoundException($"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' was not found in {Operator.ComparisonAliasLookup}.");

            return Operator.ComparisonAliasLookup[comparisonOperator] switch
            {
                Operator.In => Builders<BsonDocument>.Filter.In(field, values),
                Operator.NotIn => Builders<BsonDocument>.Filter.Nin(field, values),
                Operator.All => Builders<BsonDocument>.Filter.All(field, values),
                _ => throw new NotImplementedException($"Unable to compare {field} to {values}. {nameof(Condition.ComparisonOperator)} '{comparisonOperator}' is not implemented.")
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
                var field = sort.Field!.Camelize(Delimiter.MongoDbChildProperty);
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
                var field = include.Camelize(Delimiter.MongoDbChildProperty);

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
                var field = exclude.Camelize(Delimiter.MongoDbChildProperty);

                projectionDefinitions.Add(projectionBuilder.Exclude(field));
            }

            return projectionBuilder.Combine(projectionDefinitions);
        }

        public dynamic ChangeType(String field, Type type, String value)
        {
            try
            {
                return value.ChangeType(type);
            }
            catch (Exception)
            {
                throw new InvalidCastException($"Unable to convert value: {value} to field: {field}'s type: {type}.");
            }
        }
    }
}
