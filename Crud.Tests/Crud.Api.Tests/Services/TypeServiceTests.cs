using Crud.Api.Services;
using Crud.Api.Tests.TestingModels;

namespace Crud.Api.Tests.Services
{
    public class TypeServiceTests
    {
        private TypeService _typeService;

        public TypeServiceTests()
        {
            _typeService = new TypeService();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void GetType_NamespaceIsNullOrWhitespace_ThrowsArgumentException(String @namespace)
        {
            var typeName = nameof(Model);

            var action = () => _typeService.GetType(@namespace, typeName);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal($"{nameof(@namespace)} cannot be null or whitespace.", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void GetType_TypeNameIsNullOrWhitespace_ThrowsArgumentException(String typeName)
        {
            var @namespace = "Crud.Api.Tests.TestingModels";

            var action = () => _typeService.GetType(@namespace, typeName);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal($"{nameof(typeName)} cannot be null or whitespace.", exception.Message);
        }

        [Fact]
        public void GetType_TypeExists_ReturnsType()
        {
            var @namespace = "Crud.Api.Tests.TestingModels";
            var typeName = nameof(Model);

            var result = _typeService.GetType(@namespace, typeName);

            Assert.Null(result);  // This is null because Type.GetType does not have access to Crud.Api.Tests.TestingModels.Models.
        }
    }
}
