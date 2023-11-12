using Crud.Api.Constants;
using Crud.Api.Services;

namespace Crud.Api.Tests.Services
{
    public class SanitizerServiceTests
    {
        private SanitizerService _sanitizerService;

        public SanitizerServiceTests()
        {
            _sanitizerService = new SanitizerService();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void SanitizeTypeName_TypeNameIsNullOrWhitespace_ReturnsDefaultTypeName(String? typeName)
        {
            var result = _sanitizerService.SanitizeTypeName(typeName);

            Assert.Equal(Default.TypeName, result);
        }

        [Theory]
        [InlineData("!#$%^")]
        [InlineData("(>*-*)>")]
        public void SanitizeTypeName_SanitizedTypeNameEmpty_ReturnsDefaultTypeName(String? typeName)
        {
            var result = _sanitizerService.SanitizeTypeName(typeName);

            Assert.Equal(Default.TypeName, result);
        }

        [Theory]
        [ClassData(typeof(CleanTypeNames))]
        public void SanitizeTypeName_TypeNameIsAlreadyClean_ReturnsUnchangedTypeName(String? typeName)
        {
            var result = _sanitizerService.SanitizeTypeName(typeName);

            Assert.Equal(typeName, result);
        }

        private class CleanTypeNames : TheoryData<String?>
        {
            public CleanTypeNames()
            {
                Add("ThisIsAlreadyValid");
                Add("@if");
                Add("_1ClassName");
            }
        }
    }
}
