using System.ComponentModel.DataAnnotations.Schema;

namespace Crud.Api.Tests.Extensions
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void GetTableAttribute_TypeIsNull_ReturnsNull()
        {
            Type? type = null;

            var result = type.GetTableAttribute();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableAttribute_TypeOfModelWithoutTableAttribute_ReturnsNull()
        {
            var type = typeof(ModelWithoutTableAttribute);

            var result = type.GetTableAttribute();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableAttribute_TypeOfModelWithTableAttribute_ReturnsNameInTableAttribute()
        {
            var type = typeof(ModelWithTableAttribute);

            var result = type.GetTableAttribute();

            Assert.NotNull(result);
            Assert.Equal(nameof(ModelWithTableAttribute), result.Name);
        }

        [Fact]
        public void GetTableName_TypeIsNull_ReturnsNull()
        {
            Type? type = null;

            var result = type.GetTableName();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableName_TableAttributeIsNotNull_ReturnsNameInTableAttribute()
        {
            var type = typeof(ModelWithTableAttribute);

            var result = type.GetTableName();

            Assert.Equal(nameof(ModelWithTableAttribute), result);
        }

        [Fact]
        public void GetTableName_TableAttributeIsNull_ReturnsPluralizedNameOfClass()
        {
            var type = typeof(ModelWithoutTableAttribute);

            var result = type.GetTableName();

            Assert.Equal("ModelWithoutTableAttributes", result);
        }

        [Fact]
        public void GetPluralizedName_TypeIsNull_ReturnsNull()
        {
            Type? type = null;

            var result = type.GetPluralizedName();

            Assert.Null(result);
        }

        [Fact]
        public void GetPluralizedName_TypeIsNotNull_ReturnsPluralizedNameOfClass()
        {
            Type? type = typeof(ModelWithoutTableAttribute);

            var result = type.GetPluralizedName();

            Assert.Equal("ModelWithoutTableAttributes", result);
        }

        [Table(nameof(ModelWithTableAttribute))]
        private class ModelWithTableAttribute
        {
            public Int32 Id { get; set; }
        }

        private class ModelWithoutTableAttribute
        {
            public Int32 Id { get; set; }
        }
    }
}
