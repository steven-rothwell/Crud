using Crud.Api.Constants;

namespace Crud.Api.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void ChangeType_ValueIsNull_ReturnsNull()
        {
            string value = null;
            Type type = typeof(Nullable<int>);

            var result = value.ChangeType(type);

            Assert.Null(result);
        }

        [Fact]
        public void ChangeType_TypeIsNull_ThrowsArgumentNullException()
        {
            string value = "1";
            Type type = null;

            var action = () => value.ChangeType(type);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("type", exception.ParamName);
        }

        [Fact]
        public void ChangeType_TypeIsNullable_ReturnsUnderlyingType()
        {
            string value = "1";
            Type type = typeof(Nullable<int>);

            var result = value.ChangeType(type);

            Assert.IsType(typeof(int), result);
            Assert.Equal(1, result);
        }

        [Fact]
        public void ChangeType_TypeIsGuid_ReturnsGuid()
        {
            string value = "00000000-0000-0000-0000-000000000000";
            Type type = typeof(Guid);

            var result = value.ChangeType(type);

            Assert.IsType(typeof(Guid), result);
            Assert.Equal(Guid.Empty, result);
        }

        [Fact]
        public void ChangeType_TypeIsNotSpecial_ReturnsConvertedValue()
        {
            string value = "1";
            Type type = typeof(int);

            var result = value.ChangeType(type);

            Assert.IsType(typeof(int), result);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Pascalize_ValueIsNull_ThrowsArgumentNullException()
        {
            string value = null;
            char delimiter = Delimiter.MongoDbChildProperty;

            var action = () => value.Pascalize(delimiter);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void Pascalize_NoDelimiterInValue_ReturnsPascalizedValue()
        {
            char delimiter = Delimiter.MongoDbChildProperty;
            string value = "thisDoesNotContainADelimiter";

            var result = value.Pascalize(delimiter);

            Assert.Equal("ThisDoesNotContainADelimiter", result);
        }

        [Fact]
        public void Pascalize_DelimiterInValue_ReturnsPascalizedValue()
        {
            char delimiter = Delimiter.MongoDbChildProperty;
            string value = $"this{delimiter}does{delimiter}contain{delimiter}a{delimiter}delimiter";

            var result = value.Pascalize(delimiter);

            Assert.Equal($"This{delimiter}Does{delimiter}Contain{delimiter}A{delimiter}Delimiter", result);
        }

        [Fact]
        public void GetValueAfterFirstDelimiter_ValueIsNull_ThrowsArgumentNullException()
        {
            string value = null;
            char delimiter = Delimiter.MongoDbChildProperty;

            var action = () => value.GetValueAfterFirstDelimiter(delimiter);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void GetValueAfterFirstDelimiter_DelimiterInValue_ReturnsValueAfterLastDelimiter()
        {
            char delimiter = Delimiter.MongoDbChildProperty;
            string value = $"this{delimiter}does{delimiter}contain{delimiter}a{delimiter}delimiter";

            var result = value.GetValueAfterFirstDelimiter(delimiter);

            Assert.Equal($"does{delimiter}contain{delimiter}a{delimiter}delimiter", result);
        }

        [Fact]
        public void GetValueAfterFirstDelimiter_NoDelimiterInValue_ReturnsValue()
        {
            char delimiter = Delimiter.MongoDbChildProperty;
            string value = "thisDoesNotContainADelimiter";

            var result = value.GetValueAfterFirstDelimiter(delimiter);

            Assert.Equal(value, result);
        }

        [Fact]
        public void GetValueAfterLastDelimiter_ValueIsNull_ThrowsArgumentNullException()
        {
            string value = null;
            char delimiter = Delimiter.MongoDbChildProperty;

            var action = () => value.GetValueAfterLastDelimiter(delimiter);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void GetValueAfterLastDelimiter_DelimiterInValue_ReturnsValueAfterLastDelimiter()
        {
            char delimiter = Delimiter.MongoDbChildProperty;
            string value = $"this{delimiter}does{delimiter}contain{delimiter}a{delimiter}delimiter";

            var result = value.GetValueAfterLastDelimiter(delimiter);

            Assert.Equal("delimiter", result);
        }

        [Fact]
        public void GetValueAfterLastDelimiter_NoDelimiterInValue_ReturnsValue()
        {
            char delimiter = Delimiter.MongoDbChildProperty;
            string value = "thisDoesNotContainADelimiter";

            var result = value.GetValueAfterLastDelimiter(delimiter);

            Assert.Equal(value, result);
        }
    }
}
