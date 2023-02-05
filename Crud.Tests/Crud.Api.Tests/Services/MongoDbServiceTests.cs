using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Crud.Api.Constants;
using Crud.Api.Models;
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
        public void GetTableName_TableNameIsNull_ThrowsException()
        {
            Type type = null;

            var action = () => _mongoDbService.GetTableName(type);

            var exception = Assert.Throws<Exception>(action);
            Assert.Equal($"No table name found on type {type?.Name}.", exception.Message);
        }

        [Fact]
        public void GetTableName_TableNameIsNotNull_ReturnsTableName()
        {
            Type type = typeof(ModelWithTableAttribute);

            var result = _mongoDbService.GetTableName(type);

            Assert.Equal(nameof(ModelWithTableAttribute), result);
        }

        [Fact]
        public void GetIdFilter_ModelImplementsIExternalEntity_ReturnsFilterOnExternalId()
        {
            Type type = typeof(ModelImplementsIExternalEntity);
            Guid id = Guid.Empty;

            var result = _mongoDbService.GetIdFilter(type, id);

            Assert.NotNull(result);

            var expectedFilter = Builders<BsonDocument>.Filter.Eq(nameof(IExternalEntity.ExternalId), id);
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetIdFilter_ModelDoesNotImplementIExternalEntity_ReturnsFilterOnId()
        {
            Type type = typeof(ModelDoesNotImplementIExternalEntity);
            Guid id = Guid.Empty;

            var result = _mongoDbService.GetIdFilter(type, id);

            Assert.NotNull(result);

            var expectedFilter = Builders<BsonDocument>.Filter.Eq("Id", id);
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Theory]
        [ClassData(typeof(QueryParamsIsNullOrEmpty))]
        public void GetQueryParamFilter_QueryParamsIsNullOrEmpty_ReturnsEmptyFilter(IDictionary<String, String>? queryParams)
        {
            Type type = typeof(ModelDoesNotImplementIExternalEntity);

            var result = _mongoDbService.GetQueryParamFilter(type, queryParams);

            Assert.NotNull(result);

            var expectedFilter = new BsonDocument();
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetQueryParamFilter_QueryParamsIsNotNullOrEmpty_ReturnsFilterPerQueryParam()
        {
            Type type = typeof(ModelDoesNotImplementIExternalEntity);
            var externalId = Guid.Empty;
            var name = "Name";
            var total = 1;
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { nameof(ModelDoesNotImplementIExternalEntity.ExternalId), externalId.ToString() },
                { nameof(ModelDoesNotImplementIExternalEntity.Name), name },
                { nameof(ModelDoesNotImplementIExternalEntity.Total), total.ToString() }
            };

            var result = _mongoDbService.GetQueryParamFilter(type, queryParams);

            Assert.NotNull(result);

            FilterDefinition<BsonDocument> expectedFilter = new BsonDocument();
            expectedFilter &= Builders<BsonDocument>.Filter.Eq(nameof(ModelDoesNotImplementIExternalEntity.ExternalId), externalId);
            expectedFilter &= Builders<BsonDocument>.Filter.Eq(nameof(ModelDoesNotImplementIExternalEntity.Name), name);
            expectedFilter &= Builders<BsonDocument>.Filter.Eq(nameof(ModelDoesNotImplementIExternalEntity.Total), total);
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetAllPropertiesToUpdate_JsonNodeIsNotJsonObject_ReturnsUpdateDefinitionsWithOneUpdateOfPropertyNameAndValue()
        {
            var propertyName = nameof(ModelDoesNotImplementIExternalEntity.ExternalId);
            Type type = typeof(ModelDoesNotImplementIExternalEntity);
            var value = Guid.Empty;
            JsonNode jsonNode = JsonSerializer.SerializeToNode(value, typeof(Guid))!;

            var result = _mongoDbService.GetAllPropertiesToUpdate(propertyName, type, jsonNode);

            Assert.NotNull(result);

            var expectedUpdates = new List<UpdateDefinition<BsonDocument>>();
            expectedUpdates.Add(Builders<BsonDocument>.Update.Set(propertyName, value));
            var expectedJson = ConvertUpdatesToJson(expectedUpdates);
            var resultJson = ConvertUpdatesToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetAllPropertiesToUpdate_JsonNodeIsJsonObject_ReturnsUpdateDefinitionsWithOneUpdateOfPropertyNameAndValue()
        {
            var propertyName = nameof(ModelDoesNotImplementIExternalEntity.ChildModel);
            Type type = typeof(ModelDoesNotImplementIExternalEntity);
            var name = "ChildName";
            var value = new ChildModel { Name = name };
            JsonNode jsonNode = JsonSerializer.SerializeToNode(value, typeof(ChildModel))!;

            var result = _mongoDbService.GetAllPropertiesToUpdate(propertyName, type, jsonNode);

            Assert.NotNull(result);

            var expectedUpdates = new List<UpdateDefinition<BsonDocument>>();
            expectedUpdates.Add(Builders<BsonDocument>.Update.Set($"{nameof(ModelDoesNotImplementIExternalEntity.ChildModel)}{Delimiter.MongoDbChildProperty}{nameof(ChildModel.Name)}", name));
            var expectedJson = ConvertUpdatesToJson(expectedUpdates);
            var resultJson = ConvertUpdatesToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetConditionFilter_ConditionIsNull_ReturnsEmptyFilterDefinition()
        {
            var type = typeof(Model);
            Condition? condition = null;

            var result = _mongoDbService.GetConditionFilter(type, condition);

            Assert.NotNull(result);

            FilterDefinition<BsonDocument> expectedFilter = new BsonDocument();
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetConditionFilter_SingleCondition_ReturnsSimpleFilterDefinition()
        {
            var type = typeof(Model);
            var field = nameof(Model.Id);
            var value = 1;
            Condition? condition = new Condition
            {
                Field = field,
                ComparisonOperator = Operator.Equality,
                Value = value.ToString()
            };

            var result = _mongoDbService.GetConditionFilter(type, condition);

            Assert.NotNull(result);

            FilterDefinition<BsonDocument> expectedFilter = Builders<BsonDocument>.Filter.Eq(field, value);
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetConditionFilter_GroupedConditionsRootLogicalOperatorIsNull_ReturnsAndOfGroupedConditionsFilterDefinition()
        {
            var type = typeof(Model);
            var field1 = nameof(Model.Id);
            var value1 = 1;
            var field2 = nameof(Model.Id);
            var value2 = 2;
            var field3 = nameof(Model.Id);
            var value3 = 3;
            Condition? condition = new Condition
            {
                GroupedConditions = new List<GroupedCondition>
                {
                    new GroupedCondition
                    {
                        LogicalOperator = Operator.Or,
                        Conditions = new List<Condition>
                        {
                            new Condition
                            {
                                Field = field1,
                                ComparisonOperator = Operator.Equality,
                                Value = value1.ToString()
                            },
                            new Condition
                            {
                                Field = field2,
                                ComparisonOperator = Operator.Equality,
                                Value = value2.ToString()
                            },
                            new Condition
                            {
                                Field = field3,
                                ComparisonOperator = Operator.Equality,
                                Value = value3.ToString()
                            }
                        }
                    }
                }
            };

            var result = _mongoDbService.GetConditionFilter(type, condition);

            Assert.NotNull(result);

            var groupedConditionFilters = new List<FilterDefinition<BsonDocument>>
            {
                Builders<BsonDocument>.Filter.Eq(field1, value1),
                Builders<BsonDocument>.Filter.Eq(field2, value2),
                Builders<BsonDocument>.Filter.Eq(field3, value3)
            };
            var filter = Builders<BsonDocument>.Filter.Or(groupedConditionFilters);
            FilterDefinition<BsonDocument> expectedFilter = Builders<BsonDocument>.Filter.And(filter);
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetConditionFilter_GroupedConditionsRootLogicalOperatorIsNotNull_ReturnsRootLogicalOperatorOfGroupedConditionsFilterDefinition()
        {
            var type = typeof(Model);
            var field1 = nameof(Model.Id);
            var value1 = 1;
            var field2 = nameof(Model.Id);
            var value2 = 2;
            var field3 = nameof(Model.Id);
            var value3 = 3;
            var rootLogicalOperator = Operator.Or;
            Condition? condition = new Condition
            {
                GroupedConditions = new List<GroupedCondition>
                {
                    new GroupedCondition
                    {
                        LogicalOperator = Operator.Or,
                        Conditions = new List<Condition>
                        {
                            new Condition
                            {
                                Field = field1,
                                ComparisonOperator = Operator.Equality,
                                Value = value1.ToString()
                            },
                            new Condition
                            {
                                Field = field2,
                                ComparisonOperator = Operator.Equality,
                                Value = value2.ToString()
                            },
                            new Condition
                            {
                                Field = field3,
                                ComparisonOperator = Operator.Equality,
                                Value = value3.ToString()
                            }
                        }
                    }
                }
            };

            var result = _mongoDbService.GetConditionFilter(type, condition, rootLogicalOperator);

            Assert.NotNull(result);

            var groupedConditionFilters = new List<FilterDefinition<BsonDocument>>
            {
                Builders<BsonDocument>.Filter.Eq(field1, value1),
                Builders<BsonDocument>.Filter.Eq(field2, value2),
                Builders<BsonDocument>.Filter.Eq(field3, value3)
            };
            var filter = Builders<BsonDocument>.Filter.Or(groupedConditionFilters);
            FilterDefinition<BsonDocument> expectedFilter = Builders<BsonDocument>.Filter.Or(filter);
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetConditionsFilters_GroupedConditionsIsNull_ReturnsListOfEmptyFilterDefinition()
        {
            var type = typeof(Model);
            IReadOnlyCollection<GroupedCondition>? groupedConditions = null;

            var result = _mongoDbService.GetConditionsFilters(type, groupedConditions);

            Assert.NotNull(result);

            var expectedFilters = new List<FilterDefinition<BsonDocument>> { new BsonDocument() };
            var expectedJson = ConvertFiltersToJson(expectedFilters);
            var resultJson = ConvertFiltersToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetConditionsFilters_GroupedConditionIsNull_ReturnsListOfOtherFilterDefinitions()
        {
            var type = typeof(Model);
            var field1 = nameof(Model.Id);
            var value1 = 1;
            var field2 = nameof(Model.Id);
            var value2 = 2;
            var field3 = nameof(Model.Id);
            var value3 = 3;
            IReadOnlyCollection<GroupedCondition>? groupedConditions = new List<GroupedCondition>
            {
                (GroupedCondition)null,
                new GroupedCondition
                {
                    LogicalOperator = Operator.Or,
                    Conditions = new List<Condition>
                    {
                        new Condition
                        {
                            Field = field1,
                            ComparisonOperator = Operator.Equality,
                            Value = value1.ToString()
                        },
                        new Condition
                        {
                            Field = field2,
                            ComparisonOperator = Operator.Equality,
                            Value = value2.ToString()
                        },
                        new Condition
                        {
                            Field = field3,
                            ComparisonOperator = Operator.Equality,
                            Value = value3.ToString()
                        }
                    }
                }
            };

            var result = _mongoDbService.GetConditionsFilters(type, groupedConditions);

            Assert.NotNull(result);

            var groupedConditionFilters = new List<FilterDefinition<BsonDocument>>
            {
                Builders<BsonDocument>.Filter.Eq(field1, value1),
                Builders<BsonDocument>.Filter.Eq(field2, value2),
                Builders<BsonDocument>.Filter.Eq(field3, value3)
            };
            var filter = Builders<BsonDocument>.Filter.Or(groupedConditionFilters);
            var expectedFilters = new List<FilterDefinition<BsonDocument>> { filter };
            var expectedJson = ConvertFiltersToJson(expectedFilters);
            var resultJson = ConvertFiltersToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetConditionsFilters_GroupedConditionLogicalOperatorIsNull_ReturnsListOfOtherFilterDefinitions()
        {
            var type = typeof(Model);
            var field1 = nameof(Model.Id);
            var value1 = 1;
            var field2 = nameof(Model.Id);
            var value2 = 2;
            var field3 = nameof(Model.Id);
            var value3 = 3;
            IReadOnlyCollection<GroupedCondition>? groupedConditions = new List<GroupedCondition>
            {
                new GroupedCondition { LogicalOperator = null },
                new GroupedCondition
                {
                    LogicalOperator = Operator.Or,
                    Conditions = new List<Condition>
                    {
                        new Condition
                        {
                            Field = field1,
                            ComparisonOperator = Operator.Equality,
                            Value = value1.ToString()
                        },
                        new Condition
                        {
                            Field = field2,
                            ComparisonOperator = Operator.Equality,
                            Value = value2.ToString()
                        },
                        new Condition
                        {
                            Field = field3,
                            ComparisonOperator = Operator.Equality,
                            Value = value3.ToString()
                        }
                    }
                }
            };

            var result = _mongoDbService.GetConditionsFilters(type, groupedConditions);

            Assert.NotNull(result);

            var groupedConditionFilters = new List<FilterDefinition<BsonDocument>>
            {
                Builders<BsonDocument>.Filter.Eq(field1, value1),
                Builders<BsonDocument>.Filter.Eq(field2, value2),
                Builders<BsonDocument>.Filter.Eq(field3, value3)
            };
            var filter = Builders<BsonDocument>.Filter.Or(groupedConditionFilters);
            var expectedFilters = new List<FilterDefinition<BsonDocument>> { filter };
            var expectedJson = ConvertFiltersToJson(expectedFilters);
            var resultJson = ConvertFiltersToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetConditionsFilters_GroupedConditionConditionsIsNull_ReturnsListOfOtherFilterDefinitions()
        {
            var type = typeof(Model);
            var field1 = nameof(Model.Id);
            var value1 = 1;
            var field2 = nameof(Model.Id);
            var value2 = 2;
            var field3 = nameof(Model.Id);
            var value3 = 3;
            IReadOnlyCollection<GroupedCondition>? groupedConditions = new List<GroupedCondition>
            {
                new GroupedCondition
                {
                    LogicalOperator = Operator.And,
                    Conditions = null
                },
                new GroupedCondition
                {
                    LogicalOperator = Operator.Or,
                    Conditions = new List<Condition>
                    {
                        new Condition
                        {
                            Field = field1,
                            ComparisonOperator = Operator.Equality,
                            Value = value1.ToString()
                        },
                        new Condition
                        {
                            Field = field2,
                            ComparisonOperator = Operator.Equality,
                            Value = value2.ToString()
                        },
                        new Condition
                        {
                            Field = field3,
                            ComparisonOperator = Operator.Equality,
                            Value = value3.ToString()
                        }
                    }
                }
            };

            var result = _mongoDbService.GetConditionsFilters(type, groupedConditions);

            Assert.NotNull(result);

            var groupedConditionFilters = new List<FilterDefinition<BsonDocument>>
            {
                Builders<BsonDocument>.Filter.Eq(field1, value1),
                Builders<BsonDocument>.Filter.Eq(field2, value2),
                Builders<BsonDocument>.Filter.Eq(field3, value3)
            };
            var filter = Builders<BsonDocument>.Filter.Or(groupedConditionFilters);
            var expectedFilters = new List<FilterDefinition<BsonDocument>> { filter };
            var expectedJson = ConvertFiltersToJson(expectedFilters);
            var resultJson = ConvertFiltersToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetConditionsFilters_NoGroupedConditionsSkipped_ReturnsListOfFilterDefinitions()
        {
            var type = typeof(Model);
            var field1 = nameof(Model.Id);
            var value1 = 1;
            var field2 = nameof(Model.Id);
            var value2 = 2;
            var field3 = nameof(Model.Id);
            var value3 = 3;
            IReadOnlyCollection<GroupedCondition>? groupedConditions = new List<GroupedCondition>
            {
                new GroupedCondition
                {
                    LogicalOperator = Operator.And,
                    Conditions = new List<Condition>
                    {
                        new Condition
                        {
                            Field = field1,
                            ComparisonOperator = Operator.Equality,
                            Value = value1.ToString()
                        },
                        new Condition
                        {
                            Field = field2,
                            ComparisonOperator = Operator.Equality,
                            Value = value2.ToString()
                        },
                        new Condition
                        {
                            Field = field3,
                            ComparisonOperator = Operator.Equality,
                            Value = value3.ToString()
                        }
                    }
                },
                new GroupedCondition
                {
                    LogicalOperator = Operator.Or,
                    Conditions = new List<Condition>
                    {
                        new Condition
                        {
                            Field = field1,
                            ComparisonOperator = Operator.Equality,
                            Value = value1.ToString()
                        },
                        new Condition
                        {
                            Field = field2,
                            ComparisonOperator = Operator.Equality,
                            Value = value2.ToString()
                        },
                        new Condition
                        {
                            Field = field3,
                            ComparisonOperator = Operator.Equality,
                            Value = value3.ToString()
                        }
                    }
                }
            };

            var result = _mongoDbService.GetConditionsFilters(type, groupedConditions);

            Assert.NotNull(result);

            var groupedConditionFilters = new List<FilterDefinition<BsonDocument>>
            {
                Builders<BsonDocument>.Filter.Eq(field1, value1),
                Builders<BsonDocument>.Filter.Eq(field2, value2),
                Builders<BsonDocument>.Filter.Eq(field3, value3)
            };
            var filter1 = Builders<BsonDocument>.Filter.And(groupedConditionFilters);
            var filter2 = Builders<BsonDocument>.Filter.Or(groupedConditionFilters);
            var expectedFilters = new List<FilterDefinition<BsonDocument>> { filter1, filter2 };
            var expectedJson = ConvertFiltersToJson(expectedFilters);
            var resultJson = ConvertFiltersToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetLogicalOperatorFilter_LogicalAliasLookupDoesNotContainLogicalOperator_ThrowsKeyNotFoundException()
        {
            var logicalOperator = "NotInAliasLookup";
            var filters = new List<FilterDefinition<BsonDocument>>();

            var action = () => _mongoDbService.GetLogicalOperatorFilter(logicalOperator, filters);

            var exception = Assert.Throws<KeyNotFoundException>(action);
            Assert.Equal($"{nameof(GroupedCondition.LogicalOperator)} '{logicalOperator}' was not found in {Operator.LogicalAliasLookup}.", exception.Message);
        }

        [Theory]
        [ClassData(typeof(AndAliasFound))]
        public void GetLogicalOperatorFilter_AndAliasFound_ReturnsAndFilterDefinition(String logicalOperator)
        {
            var filters = new List<FilterDefinition<BsonDocument>>();

            var result = _mongoDbService.GetLogicalOperatorFilter(logicalOperator, filters);

            Assert.NotNull(result);

            var expectedFilter = Builders<BsonDocument>.Filter.And(filters);
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Theory]
        [ClassData(typeof(OrAliasFound))]
        public void GetLogicalOperatorFilter_OrAliasFound_ReturnsOrFilterDefinition(String logicalOperator)
        {
            var filters = new List<FilterDefinition<BsonDocument>>();

            var result = _mongoDbService.GetLogicalOperatorFilter(logicalOperator, filters);

            Assert.NotNull(result);

            var expectedFilter = Builders<BsonDocument>.Filter.Or(filters);
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Fact]
        public void GetComparisonOperatorFilter_ComparisonAliasLookupDoesNotContainComparisonOperator_ThrowsKeyNotFoundException()
        {
            var field = "Field";
            var comparisonOperator = "NotInAliasLookup";
            var value = "Value";

            var action = () => _mongoDbService.GetComparisonOperatorFilter(field, comparisonOperator, value);

            var exception = Assert.Throws<KeyNotFoundException>(action);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' was not found in {Operator.ComparisonAliasLookup}.", exception.Message);
        }

        [Theory]
        [ClassData(typeof(EqualityAliasFound))]
        public void GetComparisonOperatorFilter_EqualityOperatorFound_ThrowsKeyNotFoundException(String comparisonOperator)
        {
            var field = "Field";
            var value = "Value";

            var result = _mongoDbService.GetComparisonOperatorFilter(field, comparisonOperator, value);

            Assert.NotNull(result);

            var expectedFilter = Builders<BsonDocument>.Filter.Eq(field, value);
            var expectedJson = ConvertFilterToJson(expectedFilter);
            var resultJson = ConvertFilterToJson(result);

            Assert.Equal(expectedJson, resultJson);
        }

        [Table(nameof(ModelWithTableAttribute))]
        private class ModelWithTableAttribute
        {
            public Int32 Id { get; set; }
        }

        private class ModelImplementsIExternalEntity : IExternalEntity
        {
            public Guid? ExternalId { get; set; }
        }

        private class ModelDoesNotImplementIExternalEntity
        {
            public Guid? ExternalId { get; set; }
            public String? Name { get; set; }
            public Int32 Total { get; set; }
            public ChildModel? ChildModel { get; set; }
        }

        private class ChildModel
        {
            public String? Name { get; set; }
        }

        private class QueryParamsIsNullOrEmpty : TheoryData<IDictionary<String, String>?>
        {
            public QueryParamsIsNullOrEmpty()
            {
                Add(null);
                Add(new Dictionary<string, string>());
            }
        }

        private class AndAliasFound : TheoryData<String>
        {
            public AndAliasFound()
            {
                Add(Operator.And);
                Add("AND");
                Add("and");
            }
        }

        private class OrAliasFound : TheoryData<String>
        {
            public OrAliasFound()
            {
                Add(Operator.Or);
                Add("OR");
                Add("or");
            }
        }

        private class EqualityAliasFound : TheoryData<String>
        {
            public EqualityAliasFound()
            {
                Add(Operator.Equality);
                Add("EQUALS");
                Add("equals");
            }
        }

        private String ConvertFilterToJson(FilterDefinition<BsonDocument> filter)
        {
            var serializerRegistry = MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
            return filter.Render(documentSerializer, serializerRegistry).ToJson();
        }

        private String ConvertFiltersToJson(IEnumerable<FilterDefinition<BsonDocument>> filters)
        {
            var serializerRegistry = MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();

            var jsonBuilder = new StringBuilder();

            foreach (var filter in filters)
            {
                jsonBuilder.Append(filter.Render(documentSerializer, serializerRegistry).ToJson());
            }

            return jsonBuilder.ToString();
        }

        private String ConvertUpdatesToJson(IEnumerable<UpdateDefinition<BsonDocument>> updates)
        {
            var serializerRegistry = MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
            var jsonBuilder = new StringBuilder();

            foreach (var update in updates)
            {
                jsonBuilder.Append(update.Render(documentSerializer, serializerRegistry).ToJson());
            }

            return jsonBuilder.ToString();
        }
    }
}
