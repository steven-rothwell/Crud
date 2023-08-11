using System.ComponentModel.DataAnnotations;
using Crud.Api.Constants;

namespace Crud.Api.Tests.Extensions
{
    public class ObjectExtensionsTests
    {
        private const Char _delimiter = Delimiter.QueryParamChildProperty;

        [Fact]
        public void ValidateDataAnnotations_ModelIsNull_ThrowsArgumentNullException()
        {
            ModelWithNoDataAnnotations? model = null;

            var action = () => model.ValidateDataAnnotations();

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("model", exception.ParamName);
        }

        [Fact]
        public void ValidateDataAnnotations_NoDataAnnotations_ReturnsTrueValidationResult()
        {
            var model = new ModelWithNoDataAnnotations();

            var result = model.ValidateDataAnnotations();

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDataAnnotations_ModelHasInvalidDataAnnotations_ReturnsFalseValidationResultWithFirstFailedMessage()
        {
            var model = new ModelWithDataAnnotations();
            var validateChildModels = false;

            var result = model.ValidateDataAnnotations(validateChildModels);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ModelWithDataAnnotations.FirstName)} field is required.", result.Message);
        }

        [Fact]
        public void ValidateDataAnnotations_ModelHasValidDataAnnotations_ReturnsTrueValidationResult()
        {
            var model = new ModelWithDataAnnotations
            {
                FirstName = "FirstName",
                LastName = "LastName"
            };
            var validateChildModels = false;

            var result = model.ValidateDataAnnotations(validateChildModels);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDataAnnotations_ChildHasInvalidDataAnnotationsValidateChildModelsIsFalse_ReturnsTrueValidationResult()
        {
            var model = new ModelWithDataAnnotations
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Child = new ChildModelWithDataAnnotations()
            };
            var validateChildModels = false;

            var result = model.ValidateDataAnnotations(validateChildModels);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDataAnnotations_ChildHasInvalidDataAnnotationsValidateChildModelsIsTrue_ReturnsFalseValidationResultWithFirstFailedMessage()
        {
            var model = new ModelWithDataAnnotations
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Child = new ChildModelWithDataAnnotations()
            };

            var result = model.ValidateDataAnnotations();

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ChildModelWithDataAnnotations.Description)} field is required.", result.Message);
        }

        [Fact]
        public void ValidateDataAnnotations_DerivedHasInvalidDataAnnotations_ReturnsFalseValidationResultWithFirstFailedMessage()
        {
            var model = new DerivedModelWithDataAnnotations
            {
                FirstName = "FirstName",
                LastName = "LastName"
            };

            var result = model.ValidateDataAnnotations();

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(DerivedModelWithDataAnnotations.DerivedName)} field is required.", result.Message);
        }

        [Theory]
        [ClassData(typeof(ModelPropertiesToValidateContainsNoInvalidMemberNames))]
        public void ValidateDataAnnotations_ModelPropertiesToValidateContainsNoInvalidMemberNames_ReturnsTrueValidationResult(IReadOnlyCollection<String> propertiesToValidate)
        {
            var model = new ModelWithDataAnnotations();
            var validateChildModels = false;

            var result = model.ValidateDataAnnotations(validateChildModels, propertiesToValidate);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDataAnnotations_ModelPropertiesToValidateContainsInvalidMemberNames_ReturnsFalseValidationResultWithFirstFailedMessage()
        {
            var model = new ModelWithDataAnnotations();
            var validateChildModels = false;
            IReadOnlyCollection<string> propertiesToValidate = new List<string>
            {
                nameof(ModelWithDataAnnotations.FirstName)
            };

            var result = model.ValidateDataAnnotations(validateChildModels, propertiesToValidate);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ModelWithDataAnnotations.FirstName)} field is required.", result.Message);
        }

        [Theory]
        [ClassData(typeof(ChildPropertiesToValidateContainsNoInvalidMemberNames))]
        public void ValidateDataAnnotations_ChildPropertiesToValidateContainsNoInvalidMemberNames_ReturnsTrueValidationResult(IReadOnlyCollection<String> propertiesToValidate)
        {
            var model = new ModelWithDataAnnotations
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Child = new ChildModelWithDataAnnotations()
            };
            var validateChildModels = true;

            var result = model.ValidateDataAnnotations(validateChildModels, propertiesToValidate, _delimiter);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDataAnnotations_ChildPropertiesToValidateContainsInvalidMemberNames_ReturnsFalseValidationResultWithFirstFailedMessage()
        {
            var model = new ModelWithDataAnnotations
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Child = new ChildModelWithDataAnnotations()
            };
            var validateChildModels = true;
            IReadOnlyCollection<String> propertiesToValidate = new List<string>
            {
                $"{nameof(ModelWithDataAnnotations.Child)}{_delimiter}{nameof(ChildModelWithDataAnnotations.Description)}"
            };

            var result = model.ValidateDataAnnotations(validateChildModels, propertiesToValidate, _delimiter);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ChildModelWithDataAnnotations.Description)} field is required.", result.Message);
        }

        private class ModelWithDataAnnotations
        {
            [Required]
            [StringLength(10)]
            public String? FirstName { get; set; }
            [Required]
            public String? LastName { get; set; }
            public ChildModelWithDataAnnotations? Child { get; set; }
        }

        private class ChildModelWithDataAnnotations
        {
            [Required]
            [StringLength(10)]
            public String? Description { get; set; }
        }

        private class DerivedModelWithDataAnnotations : ModelWithDataAnnotations
        {
            [Required]
            [StringLength(10)]
            public String? DerivedName { get; set; }
        }

        private class ModelWithNoDataAnnotations
        {
            public String? FirstName { get; set; }
            public String? LastName { get; set; }
        }

        private class ModelPropertiesToValidateContainsNoInvalidMemberNames : TheoryData<IReadOnlyCollection<String>>
        {
            public ModelPropertiesToValidateContainsNoInvalidMemberNames()
            {
                Add(new List<string>());
                Add(new List<string> { "PropertyDoesNotExist" });
            }
        }

        private class ChildPropertiesToValidateContainsNoInvalidMemberNames : TheoryData<IReadOnlyCollection<String>>
        {
            public ChildPropertiesToValidateContainsNoInvalidMemberNames()
            {
                Add(new List<string>());
                Add(new List<string> { $"{nameof(ModelWithDataAnnotations.Child)}{_delimiter}PropertyDoesNotExist" });
            }
        }
    }
}
