using System.Reflection;
using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.QueryModels;
using Crud.Api.Validators;
using Crud.Api.Validators.Attributes;
using Microsoft.Extensions.Options;
using Moq;
using DataAnnotations = System.ComponentModel.DataAnnotations;

namespace Crud.Api.Tests.Validators
{
    public class ValidatorTests
    {
        private Mock<IPreserver> _preserver;
        private IOptions<ApplicationOptions> _applicationOptions;
        private Validator _validator;

        public ValidatorTests()
        {
            _preserver = new Mock<IPreserver>();
            _applicationOptions = Microsoft.Extensions.Options.Options.Create(new ApplicationOptions { PreventAllQueryContains = false, PreventAllQueryStartsWith = false, PreventAllQueryEndsWith = false });

            _validator = new Validator(_preserver.Object, _applicationOptions);
        }

        [Fact]
        public async Task ValidateCreateAsync_WithObject_DataAnnotationsValidationIsInvalid_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation { Id = 1 };

            var result = await _validator.ValidateCreateAsync(model);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ModelForValidation.Name)} field is required.", result.Message);
        }

        [Fact]
        public async Task ValidateCreateAsync_WithObject_DataAnnotationsValidationIsValid_ReturnsTrueValidationResult()
        {
            object model = new ModelForValidation { Id = 1, Name = "Test" };

            var result = await _validator.ValidateCreateAsync(model);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Theory]
        [ClassData(typeof(QueryParamsIsNullOrEmpty))]
        public async Task ValidateReadAsync_WithObjectIDictionaryOfStringString_QueryParamsIsNullOrEmpty_ReturnsFalseValidationResult(IDictionary<String, String>? queryParams)
        {
            object model = new Object();

            var result = await _validator.ValidateReadAsync(model, queryParams);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Filter cannot be empty.", result.Message);
        }

        [Fact]
        public async Task ValidateReadAsync_WithObjectIDictionaryOfStringString_ModelDoesNotHaveAllPropertiesInQueryParams_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation { Id = 1 };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { "PropertyDoesNotExist", "value" }
            };

            var result = await _validator.ValidateReadAsync(model, queryParams);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Filter cannot contain properties that the model does not have.", result.Message);
        }

        [Fact]
        public async Task ValidateReadAsync_WithObjectIDictionaryOfStringString_ModelHasAllPropertiesInQueryParams_ReturnsTrueValidationResult()
        {
            object model = new ModelForValidation { Id = 1 };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { nameof(ModelForValidation.Id), "value" }
            };

            var result = await _validator.ValidateReadAsync(model, queryParams);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidateUpdateAsync_WithGuidObject_DataAnnotationsValidationIsInvalid_ReturnsFalseValidationResult()
        {
            var id = Guid.Empty;
            object model = new ModelForValidation { Id = 1 };

            var result = await _validator.ValidateUpdateAsync(id, model);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ModelForValidation.Name)} field is required.", result.Message);
        }

        [Fact]
        public async Task ValidateUpdateAsync_WithGuidObject_DataAnnotationsValidationIsValid_ReturnsTrueValidationResult()
        {
            var id = Guid.Empty;
            object model = new ModelForValidation { Id = 1, Name = "Test" };

            var result = await _validator.ValidateUpdateAsync(id, model);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Theory]
        [ClassData(typeof(PropertiesToBeUpdatedIsNullOrEmpty))]
        public async Task ValidatePartialUpdateAsync_WithGuidObjectIReadOnlyCollectionOfString_PropertiesToBeUpdatedIsNullOrEmpty_ReturnsFalseValidationResult(IReadOnlyCollection<String>? propertiesToBeUpdated)
        {
            var id = Guid.Empty;
            object model = new Object();

            var result = await _validator.ValidatePartialUpdateAsync(id, model, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Updated properties cannot be empty.", result.Message);
        }

        [Fact]
        public async Task ValidatePartialUpdateAsync_WithGuidObjectIReadOnlyCollectionOfString_ModelDoesNotHaveAllPropertiesInPropertiesToBeUpdated_ReturnsFalseValidationResult()
        {
            var id = Guid.Empty;
            object model = new ModelForValidation { Id = 1 };
            IReadOnlyCollection<String>? propertiesToBeUpdated = new List<string>
            {
                "PropertyDoesNotExist"
            };

            var result = await _validator.ValidatePartialUpdateAsync(id, model, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Updated properties cannot contain properties that the model does not have.", result.Message);
        }

        [Fact]
        public async Task ValidatePartialUpdateAsync_WithGuidObjectIReadOnlyCollectionOfString_ModelHasAllPropertiesInPropertiesToBeUpdated_ReturnsTrueValidationResult()
        {
            var id = Guid.Empty;
            object model = new ModelForValidation { Id = 1 };
            IReadOnlyCollection<String>? propertiesToBeUpdated = new List<string>
            {
                nameof(ModelForValidation.Id)
            };

            var result = await _validator.ValidatePartialUpdateAsync(id, model, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidatePartialUpdateAsync_WithGuidObjectIReadOnlyCollectionOfString_DataAnnotationsValidationIsInvalid_ReturnsFalseValidationResult()
        {
            var id = Guid.Empty;
            object model = new ModelForValidation { Id = 1 };
            IReadOnlyCollection<String>? propertiesToBeUpdated = new List<string>
            {
                nameof(ModelForValidation.Id),
                nameof(ModelForValidation.Name)
            };

            var result = await _validator.ValidatePartialUpdateAsync(id, model, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ModelForValidation.Name)} field is required.", result.Message);
        }

        [Fact]
        public async Task ValidatePartialUpdateAsync_WithGuidObjectIReadOnlyCollectionOfString_DataAnnotationsValidationIsValid_ReturnsTrueValidationResult()
        {
            var id = Guid.Empty;
            object model = new ModelForValidation { Id = 1, Name = "Test" };
            IReadOnlyCollection<String>? propertiesToBeUpdated = new List<string>
            {
                nameof(ModelForValidation.Id),
                nameof(ModelForValidation.Name)
            };

            var result = await _validator.ValidatePartialUpdateAsync(id, model, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Theory]
        [ClassData(typeof(QueryParamsIsNullOrEmpty))]
        public async Task ValidatePartialUpdateAsync_WithObjectIDictionaryOfStringStringIReadOnlyCollectionOfString_QueryParamsIsNullOrEmpty_ReturnsFalseValidationResult(IDictionary<String, String>? queryParams)
        {
            object model = new Object();
            IReadOnlyCollection<string>? propertiesToBeUpdated = new List<string>();

            var result = await _validator.ValidatePartialUpdateAsync(model, queryParams, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Filter cannot be empty.", result.Message);
        }

        [Fact]
        public async Task ValidatePartialUpdateAsync_WithObjectIDictionaryOfStringStringIReadOnlyCollectionOfString_ModelDoesNotHaveAllPropertiesInQueryParams_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation { Id = 1 };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { "PropertyDoesNotExist", "value" }
            };
            IReadOnlyCollection<string>? propertiesToBeUpdated = new List<string>();

            var result = await _validator.ValidatePartialUpdateAsync(model, queryParams, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Filter cannot contain properties that the model does not have.", result.Message);
        }

        [Theory]
        [ClassData(typeof(PropertiesToBeUpdatedIsNullOrEmpty))]
        public async Task ValidatePartialUpdateAsync_WithObjectIDictionaryOfStringStringIReadOnlyCollectionOfString_PropertiesToBeUpdatedIsNullOrEmpty_ReturnsFalseValidationResult(IReadOnlyCollection<String>? propertiesToBeUpdated)
        {
            object model = new ModelForValidation { Id = 1 };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { nameof(ModelForValidation.Id), "value" }
            };

            var result = await _validator.ValidatePartialUpdateAsync(model, queryParams, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Updated properties cannot be empty.", result.Message);
        }

        [Fact]
        public async Task ValidatePartialUpdateAsync_WithObjectIDictionaryOfStringStringIReadOnlyCollectionOfString_ModelDoesNotHaveAllPropertiesInPropertiesToBeUpdated_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation { Id = 1 };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { nameof(ModelForValidation.Id), "value" }
            };
            IReadOnlyCollection<string>? propertiesToBeUpdated = new List<string>
            {
                "PropertyDoesNotExist"
            };

            var result = await _validator.ValidatePartialUpdateAsync(model, queryParams, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Updated properties cannot contain properties that the model does not have.", result.Message);
        }

        [Fact]
        public async Task ValidatePartialUpdateAsync_WithObjectIDictionaryOfStringStringIReadOnlyCollectionOfString_DataAnnotationsValidationIsInvalid_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation { Id = 1 };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { nameof(ModelForValidation.Id), "value" }
            };
            IReadOnlyCollection<String>? propertiesToBeUpdated = new List<string>
            {
                nameof(ModelForValidation.Id),
                nameof(ModelForValidation.Name)
            };

            var result = await _validator.ValidatePartialUpdateAsync(model, queryParams, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ModelForValidation.Name)} field is required.", result.Message);
        }

        [Fact]
        public async Task ValidatePartialUpdateAsync_WithObjectIDictionaryOfStringStringIReadOnlyCollectionOfString_DataAnnotationsValidationIsValid_ReturnsTrueValidationResult()
        {
            object model = new ModelForValidation { Id = 1, Name = "Test" };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { nameof(ModelForValidation.Id), "value" }
            };
            IReadOnlyCollection<String>? propertiesToBeUpdated = new List<string>
            {
                nameof(ModelForValidation.Id),
                nameof(ModelForValidation.Name)
            };

            var result = await _validator.ValidatePartialUpdateAsync(model, queryParams, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidatePartialUpdateAsync_WithObjectIDictionaryOfStringStringIReadOnlyCollectionOfString_ModelHasAllPropertiesInPropertiesToBeUpdated_ReturnsTrueValidationResult()
        {
            object model = new ModelForValidation { Id = 1 };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { nameof(ModelForValidation.Id), "value" }
            };
            IReadOnlyCollection<string>? propertiesToBeUpdated = new List<string>
            {
                nameof(ModelForValidation.Id)
            };

            var result = await _validator.ValidatePartialUpdateAsync(model, queryParams, propertiesToBeUpdated);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Theory]
        [ClassData(typeof(QueryParamsIsNullOrEmpty))]
        public async Task ValidateDeleteAsync_WithObjectIDictionaryOfStringString_QueryParamsIsNullOrEmpty_ReturnsFalseValidationResult(IDictionary<String, String>? queryParams)
        {
            object model = new Object();

            var result = await _validator.ValidateDeleteAsync(model, queryParams);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Filter cannot be empty.", result.Message);
        }

        [Fact]
        public async Task ValidateDeleteAsync_WithObjectIDictionaryOfStringString_ModelDoesNotHaveAllPropertiesInQueryParams_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation { Id = 1 };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { "PropertyDoesNotExist", "value" }
            };

            var result = await _validator.ValidateDeleteAsync(model, queryParams);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Filter cannot contain properties that the model does not have.", result.Message);
        }

        [Fact]
        public async Task ValidateDeleteAsync_WithObjectIDictionaryOfStringString_ModelHasAllPropertiesInQueryParams_ReturnsTrueValidationResult()
        {
            object model = new ModelForValidation { Id = 1 };
            IDictionary<string, string>? queryParams = new Dictionary<string, string>
            {
                { nameof(ModelForValidation.Id), "value" }
            };

            var result = await _validator.ValidateDeleteAsync(model, queryParams);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateQuery_IncludesIsPopulatedAndExcludesIsPopulated_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var query = new Query
            {
                Includes = new HashSet<string> { "IncludedFieldName" },
                Excludes = new HashSet<string> { "ExcludedFieldName" }
            };

            var result = _validator.ValidateQuery(model, query);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Query)} {nameof(Query.Includes)} and {nameof(Query.Excludes)} cannot both be populated.", result.Message);
        }

        [Fact]
        public void ValidateQuery_IncludesIsPopulatedAndModelDoesNotHaveAllPropertiesInIncludes_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var query = new Query
            {
                Includes = new HashSet<string> { nameof(ModelForValidation.Id), "PropertyDoesNotExist" }
            };

            var result = _validator.ValidateQuery(model, query);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Query)} {nameof(Query.Includes)} cannot contain properties that the model does not have.", result.Message);
        }

        [Fact]
        public void ValidateQuery_ExcludesIsPopulatedAndModelDoesNotHaveAllPropertiesInExcludes_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var query = new Query
            {
                Excludes = new HashSet<string> { nameof(ModelForValidation.Id), "PropertyDoesNotExist" }
            };

            var result = _validator.ValidateQuery(model, query);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Query)} {nameof(Query.Excludes)} cannot contain properties that the model does not have.", result.Message);
        }

        [Fact]
        public void ValidateQuery_WhereIsNotNullAndConditionValidationResultIsInvalid_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var field = "PropertyDoesNotExist";
            var query = new Query
            {
                Where = new Condition
                {
                    Field = field
                }
            };

            var result = _validator.ValidateQuery(model, query);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"A {nameof(Condition)} {nameof(Condition.Field)} contains a property {field} that the model does not have.", result.Message);
        }

        [Fact]
        public void ValidateQuery_OrderByIsNotNullAndOrderByValidationResultIsInvalid_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var query = new Query
            {
                OrderBy = new List<Sort>
                {
                    new Sort { Field = null }
                }
            };

            var result = _validator.ValidateQuery(model, query);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Query.OrderBy)} cannot contain a {nameof(Sort)} with a null {nameof(Sort.Field)}.", result.Message);
        }

        [Fact]
        public void ValidateQuery_LimitIsLessThanZero_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var query = new Query
            {
                Limit = -1
            };

            var result = _validator.ValidateQuery(model, query);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.", result.Message);
        }

        [Fact]
        public void ValidateQuery_SkipIsLessThanZero_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var query = new Query
            {
                Skip = -1
            };

            var result = _validator.ValidateQuery(model, query);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Query)} {nameof(Query.Skip)} cannot be less than zero.", result.Message);
        }

        [Fact]
        public void ValidateQuery_QueryIsValid_ReturnsTrueValidationResult()
        {
            object model = new ModelForValidation();
            var query = new Query
            {
                Skip = 1
            };

            var result = _validator.ValidateQuery(model, query);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateCondition_FieldIsNullAndGroupedConditionsIsNull_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var condition = new Condition
            {
                Field = null,
                GroupedConditions = null
            };

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"A {nameof(Condition)} must contain either a {nameof(Condition.Field)} or {nameof(Condition.GroupedConditions)}.", result.Message);
        }

        [Fact]
        public void ValidateCondition_ModelDoesNotHaveFieldPropertyName_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var condition = new Condition
            {
                Field = "PropertyDoesNotExist"
            };

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"A {nameof(Condition)} {nameof(Condition.Field)} contains a property {condition.Field} that the model does not have.", result.Message);
        }

        [Fact]
        public void ValidateCondition_ComparisonOperatorIsNull_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var condition = new Condition
            {
                Field = nameof(ModelForValidation.Id),
                ComparisonOperator = null
            };

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"A {nameof(Condition)} cannot have a populated {nameof(Condition.Field)} and a null {nameof(Condition.ComparisonOperator)}.", result.Message);
        }

        [Fact]
        public void ValidateCondition_ComparisonOperatorNotInComparisonAliasLookup_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var condition = new Condition
            {
                Field = nameof(ModelForValidation.Id),
                ComparisonOperator = "DoesNotExist"
            };

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{condition.ComparisonOperator}' must be found in {Operator.ComparisonAliasLookup}.", result.Message);
        }

        [Fact]
        public void ValidateCondition_OperatorNotAllowedByApplicationOptions_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var condition = new Condition
            {
                Field = nameof(ModelForValidation.Id),
                ComparisonOperator = Operator.Contains
            };
            _applicationOptions.Value.PreventAllQueryContains = true;

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{condition.ComparisonOperator}' may not be used.", result.Message);
        }

        [Fact]
        public void ValidateCondition_OperatorNotAllowedByPropertyQueryAttribute_ReturnsFalseValidationResult()
        {
            object model = new PreventQueryModel();
            var condition = new Condition
            {
                Field = nameof(PreventQueryModel.NameContains),
                ComparisonOperator = Operator.Contains
            };

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{condition.ComparisonOperator}' may not be used on the {condition.Field} property.", result.Message);
        }

        [Theory]
        [ClassData(typeof(ValueIsNotAllLettersOrNumbers))]
        public void ValidateCondition_ContainsOperatorAndValueIsNotAllLettersOrNumbers_ReturnsFalseValidationResult(String comparisonOperator, String value)
        {
            object model = new ModelForValidation();
            var condition = new Condition
            {
                Field = nameof(ModelForValidation.Name),
                ComparisonOperator = comparisonOperator,
                Value = value
            };

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{condition.ComparisonOperator}' can only contain letters and numbers.", result.Message);
        }

        [Theory]
        [ClassData(typeof(ValueIsAllLettersOrNumbers))]
        public void ValidateCondition_ContainsOperatorAndValueIsAllLettersOrNumbers_ReturnsTrueValidationResult(String comparisonOperator, String value)
        {
            object model = new ModelForValidation();
            var condition = new Condition
            {
                Field = nameof(ModelForValidation.Name),
                ComparisonOperator = comparisonOperator,
                Value = value
            };

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateQueryApplicationOptions_ComparisonOperatorIsNull_ReturnsTrueValidationResult()
        {
            string? comparisonOperator = null;

            var result = _validator.ValidateQueryApplicationOptions(comparisonOperator);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateQueryApplicationOptions_ComparisonOperatorContainsAndPreventAllQueryContains_ReturnsFalseValidationResult()
        {
            string? comparisonOperator = Operator.Contains;
            _applicationOptions.Value.PreventAllQueryContains = true;

            var result = _validator.ValidateQueryApplicationOptions(comparisonOperator);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' may not be used.", result.Message);
        }

        [Fact]
        public void ValidateQueryApplicationOptions_ComparisonOperatorStartsWithAndPreventAllQueryStartsWith_ReturnsFalseValidationResult()
        {
            string? comparisonOperator = Operator.StartsWith;
            _applicationOptions.Value.PreventAllQueryStartsWith = true;

            var result = _validator.ValidateQueryApplicationOptions(comparisonOperator);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' may not be used.", result.Message);
        }

        [Fact]
        public void ValidateQueryApplicationOptions_ComparisonOperatorEndsWithAndPreventAllQueryEndsWith_ReturnsFalseValidationResult()
        {
            string? comparisonOperator = Operator.EndsWith;
            _applicationOptions.Value.PreventAllQueryEndsWith = true;

            var result = _validator.ValidateQueryApplicationOptions(comparisonOperator);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' may not be used.", result.Message);
        }

        [Fact]
        public void ValidatePropertyQueryAttributes_ComparisonOperatorIsNull_ReturnsTrueValidationsResult()
        {
            var propertyInfo = typeof(PreventQueryModel).GetProperty(nameof(PreventQueryModel.NameContains));
            string? comparisonOperator = null;

            var result = _validator.ValidatePropertyQueryAttributes(propertyInfo!, comparisonOperator);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidatePropertyQueryAttributes_ComparisonOperatorContainsAndPropertyHasPreventQueryContainsAttribute_ReturnsFalseValidationsResult()
        {
            var propertyInfo = typeof(PreventQueryModel).GetProperty(nameof(PreventQueryModel.NameContains))!;
            string? comparisonOperator = Operator.Contains;

            var result = _validator.ValidatePropertyQueryAttributes(propertyInfo, comparisonOperator);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' may not be used on the {propertyInfo.Name} property.", result.Message);
        }

        [Fact]
        public void ValidatePropertyQueryAttributes_ComparisonOperatorStartsWithAndPropertyHasPreventQueryStartsWithAttribute_ReturnsFalseValidationsResult()
        {
            var propertyInfo = typeof(PreventQueryModel).GetProperty(nameof(PreventQueryModel.NameStartsWith))!;
            string? comparisonOperator = Operator.StartsWith;

            var result = _validator.ValidatePropertyQueryAttributes(propertyInfo, comparisonOperator);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' may not be used on the {propertyInfo.Name} property.", result.Message);
        }

        [Fact]
        public void ValidatePropertyQueryAttributes_ComparisonOperatorEndsWithAndPropertyHasPreventQueryEndsWithAttribute_ReturnsFalseValidationsResult()
        {
            var propertyInfo = typeof(PreventQueryModel).GetProperty(nameof(PreventQueryModel.NameEndsWith))!;
            string? comparisonOperator = Operator.EndsWith;

            var result = _validator.ValidatePropertyQueryAttributes(propertyInfo, comparisonOperator);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' may not be used on the {propertyInfo.Name} property.", result.Message);
        }

        [Fact]
        public void ValidateCondition_GroupedConditionsContainsAtLeastOneInvalidGroupedCondition_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var logicalOperator = "DoesNotExist";
            var condition = new Condition
            {
                GroupedConditions = new List<GroupedCondition>
                {
                    new GroupedCondition { LogicalOperator = logicalOperator }
                }
            };

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(GroupedCondition.LogicalOperator)} '{logicalOperator}' must be found in {Operator.LogicalAliasLookup}.", result.Message);
        }

        [Fact]
        public void ValidateCondition_ConditionIsValid_ReturnsTrueValidationResult()
        {
            object model = new ModelForValidation();
            var condition = new Condition
            {
                Field = nameof(ModelForValidation.Id),
                ComparisonOperator = Operator.Equality,
                Value = "1"
            };

            var result = _validator.ValidateCondition(model.GetType().GetProperties(), condition);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateGroupedCondition_LogicalOperatorIsNotNullAndNotInLogicalAliasLookup_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var logicalOperator = "DoesNotExist";
            var groupedCondition = new GroupedCondition { LogicalOperator = logicalOperator };

            var result = _validator.ValidateGroupedCondition(model.GetType().GetProperties(), groupedCondition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(GroupedCondition.LogicalOperator)} '{groupedCondition.LogicalOperator}' must be found in {Operator.LogicalAliasLookup}.", result.Message);
        }

        [Theory]
        [ClassData(typeof(ConditionsIsNullOrEmpty))]
        public void ValidateGroupedCondition_ConditionsIsNullOrEmpty_ReturnsFalseValidationResult(IReadOnlyCollection<Condition> conditions)
        {
            object model = new ModelForValidation();
            var groupedCondition = new GroupedCondition { Conditions = conditions };

            var result = _validator.ValidateGroupedCondition(model.GetType().GetProperties(), groupedCondition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(GroupedCondition.Conditions)} cannot be empty.", result.Message);
        }

        [Fact]
        public void ValidateGroupedCondition_ConditionsContainsAtLeastOneInvalidCondition_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var groupedCondition = new GroupedCondition
            {
                Conditions = new List<Condition>
                {
                    new Condition()
                }
            };

            var result = _validator.ValidateGroupedCondition(model.GetType().GetProperties(), groupedCondition);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"A {nameof(Condition)} must contain either a {nameof(Condition.Field)} or {nameof(Condition.GroupedConditions)}.", result.Message);
        }

        [Fact]
        public void ValidateGroupedCondition_GroupedConditionIsValid_ReturnsTrueValidationResult()
        {
            object model = new ModelForValidation();
            var groupedCondition = new GroupedCondition
            {
                Conditions = new List<Condition>
                {
                    new Condition
                    {
                        Field = nameof(ModelForValidation.Id),
                        ComparisonOperator = Operator.Equality,
                        Value = "1"
                    }
                }
            };

            var result = _validator.ValidateGroupedCondition(model.GetType().GetProperties(), groupedCondition);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateSorts_FieldIsNull_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var sorts = new List<Sort>
            {
                new Sort { Field = null }
            };

            var result = _validator.ValidateSorts(model.GetType().GetProperties(), sorts);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"{nameof(Query.OrderBy)} cannot contain a {nameof(Sort)} with a null {nameof(Sort.Field)}.", result.Message);
        }

        [Fact]
        public void ValidateSorts_ModelDoesNotHaveFieldPropertyName_ReturnsFalseValidationResult()
        {
            object model = new ModelForValidation();
            var field = "DoesNotExist";
            var sorts = new List<Sort>
            {
                new Sort { Field = field }
            };

            var result = _validator.ValidateSorts(model.GetType().GetProperties(), sorts);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"A {nameof(Sort)} {nameof(Sort.Field)} contains a property {field} that the model does not have.", result.Message);
        }

        [Fact]
        public void ValidateSorts_SortsIsValid_ReturnsTrueValidationResult()
        {
            object model = new ModelForValidation();
            var sorts = new List<Sort>
            {
                new Sort { Field = nameof(ModelForValidation.Id) }
            };

            var result = _validator.ValidateSorts(model.GetType().GetProperties(), sorts);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        private class ModelForValidation
        {
            public Int32 Id { get; set; }
            [DataAnnotations.Required]
            public String? Name { get; set; }
        }

        private class PreventQueryModel
        {
            [PreventQueryContains]
            public String? NameContains { get; set; }
            [PreventQueryStartsWith]
            public String? NameStartsWith { get; set; }
            [PreventQueryEndsWith]
            public String? NameEndsWith { get; set; }
        }

        private class QueryParamsIsNullOrEmpty : TheoryData<IDictionary<String, String>?>
        {
            public QueryParamsIsNullOrEmpty()
            {
                Add(null);
                Add(new Dictionary<string, string>());
            }
        }

        private class PropertiesToBeUpdatedIsNullOrEmpty : TheoryData<IReadOnlyCollection<String>?>
        {
            public PropertiesToBeUpdatedIsNullOrEmpty()
            {
                Add(null);
                Add(new List<string>());
            }
        }

        private class ConditionsIsNullOrEmpty : TheoryData<IReadOnlyCollection<Condition>?>
        {
            public ConditionsIsNullOrEmpty()
            {
                Add(null);
                Add(new List<Condition>());
            }
        }

        private class ValueIsAllLettersOrNumbers : TheoryData<String, String>
        {
            public ValueIsAllLettersOrNumbers()
            {
                Add(Operator.Contains, "letters");
                Add(Operator.StartsWith, "12345");
                Add(Operator.EndsWith, "L3tt3r5");
            }
        }

        private class ValueIsNotAllLettersOrNumbers : TheoryData<String, String>
        {
            public ValueIsNotAllLettersOrNumbers()
            {
                Add(Operator.Contains, "letter$");
                Add(Operator.StartsWith, "");
                Add(Operator.EndsWith, " ");
            }
        }
    }
}
