using System.ComponentModel.DataAnnotations;

namespace Crud.Api.Tests.Extensions
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void ValidateDataAnnotations_WithObject_ModelIsNull_ThrowsArgumentNullException()
        {
            ModelWithNoDataAnnotations model = null;

            var action = () => model.ValidateDataAnnotations();

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("model", exception.ParamName);
        }

        [Fact]
        public void ValidateDataAnnotations_WithObject_NoDataAnnotations_ReturnsTrueValidationResult()
        {
            var model = new ModelWithNoDataAnnotations();

            var result = model.ValidateDataAnnotations();

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDataAnnotations_WithObject_HasInvalidDataAnnotations_ReturnsFalseValidationResultWithFirstFailedMessage()
        {
            var model = new ModelWithDataAnnotations();

            var result = model.ValidateDataAnnotations();

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ModelWithDataAnnotations.FirstName)} field is required.", result.Message);
        }

        [Fact]
        public void ValidateDataAnnotations_WithObject_HasValidDataAnnotations_ReturnsTrueValidationResult()
        {
            var model = new ModelWithDataAnnotations
            {
                FirstName = "FirstName",
                LastName = "LastName"
            };

            var result = model.ValidateDataAnnotations();

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDataAnnotations_WithObjectIReadOnlyCollectionOfString_ModelIsNull_ThrowsArgumentNullException()
        {
            ModelWithNoDataAnnotations model = null;
            var propertiesToValidate = new List<string>();

            var action = () => model.ValidateDataAnnotations(propertiesToValidate);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("model", exception.ParamName);
        }

        [Fact]
        public void ValidateDataAnnotations_WithObjectIReadOnlyCollectionOfString_PropertiesToValidateIsNull_ThrowsArgumentNullException()
        {
            var model = new ModelWithNoDataAnnotations();
            List<string> propertiesToValidate = null;

            var action = () => model.ValidateDataAnnotations(propertiesToValidate);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("propertiesToValidate", exception.ParamName);
        }

        [Fact]
        public void ValidateDataAnnotations_WithObjectIReadOnlyCollectionOfString_NoDataAnnotations_ReturnsTrueValidationResult()
        {
            var model = new ModelWithNoDataAnnotations();
            var propertiesToValidate = new List<string>();

            var result = model.ValidateDataAnnotations(propertiesToValidate);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Theory]
        [ClassData(typeof(PropertiesToValidateContainsNoMemberNames))]
        public void ValidateDataAnnotations_WithObjectIReadOnlyCollectionOfString_HasInvalidDataAnnotationsPropertiesToValidateContainsNoMemberNames_ReturnsTrueValidationResult(IReadOnlyCollection<String> propertiesToValidate)
        {
            var model = new ModelWithDataAnnotations();

            var result = model.ValidateDataAnnotations(propertiesToValidate);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDataAnnotations_WithObjectIReadOnlyCollectionOfString_HasInvalidDataAnnotationsPropertiesToValidateContainsMemberNames_ReturnsFalseValidationResultWithFirstFailedMessage()
        {
            var model = new ModelWithDataAnnotations();
            var propertiesToValidate = new List<string>
            {
                nameof(ModelWithDataAnnotations.FirstName),
                nameof(ModelWithDataAnnotations.LastName)
            };

            var result = model.ValidateDataAnnotations(propertiesToValidate);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal($"The {nameof(ModelWithDataAnnotations.FirstName)} field is required.", result.Message);
        }

        [Fact]
        public void ValidateDataAnnotations_WithObjectIReadOnlyCollectionOfString_HasValidDataAnnotationsPropertiesToValidateContainsMemberNames_ReturnsTrueValidationResult()
        {
            var model = new ModelWithDataAnnotations
            {
                FirstName = "FirstName",
                LastName = "LastName"
            };
            var propertiesToValidate = new List<string>
            {
                nameof(ModelWithDataAnnotations.FirstName),
                nameof(ModelWithDataAnnotations.LastName)
            };

            var result = model.ValidateDataAnnotations(propertiesToValidate);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        private class ModelWithDataAnnotations
        {
            [Required]
            [StringLength(10)]
            public String? FirstName { get; set; }
            [Required]
            public String? LastName { get; set; }
        }

        private class ModelWithNoDataAnnotations
        {
            public String? FirstName { get; set; }
            public String? LastName { get; set; }
        }

        private class PropertiesToValidateContainsNoMemberNames : TheoryData<IReadOnlyCollection<String>>
        {
            public PropertiesToValidateContainsNoMemberNames()
            {
                Add(new List<string>());
                Add(new List<string> { "PropertyDoesNotExist" });
            }
        }
    }
}
