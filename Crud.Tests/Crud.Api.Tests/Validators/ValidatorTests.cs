using Crud.Api.Preservers;
using Crud.Api.Validators;
using Moq;

namespace Crud.Api.Tests.Validators
{
    public class ValidatorTests
    {
        private Mock<IPreserver> _preserver;
        private Validator _validator;

        public ValidatorTests()
        {
            _preserver = new Mock<IPreserver>();

            _validator = new Validator(_preserver.Object);
        }

        [Fact]
        public async Task ValidateCreateAsync_WithObject_NoLogic_ReturnsTrueValidationResult()
        {
            object model = new Object();

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
        public async Task ValidateUpdateAsync__WithGuidObject_NoLogic_ReturnsTrueValidationResult()
        {
            var id = Guid.Empty;
            object model = new Object();

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

        private class ModelForValidation
        {
            public Int32 Id { get; set; }
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
    }
}
