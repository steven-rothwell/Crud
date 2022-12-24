using System.ComponentModel.DataAnnotations.Schema;
using Crud.Api.Tests.TestingModels;

namespace Crud.Api.Tests.Extensions
{
    public class GenericExtensionsTests
    {
        [Fact]
        public void GetTableAttribute_WithT_TIsNull_ReturnsNull()
        {
            Model model = null;

            var result = model.GetTableAttribute();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableAttribute_WithT_ModelWithoutTableAttribute_ReturnsNull()
        {
            var model = new ModelWithoutTableAttribute { Id = 1 };

            var result = model.GetTableAttribute();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableAttribute_WithT_ModelWithTableAttribute_ReturnsTableAttributeValue()
        {
            var model = new ModelWithTableAttribute { Id = 1 };

            var result = model.GetTableAttribute();

            Assert.NotNull(result);
            Assert.Equal(nameof(ModelWithTableAttribute), result.Name);
        }

        [Fact]
        public void GetTableName_WithT_GetTableAttributeReturnsNull_ReturnsNull()
        {
            Model model = null;

            var result = model.GetTableName();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableName_WithT_GetTableAttributeReturnsNotNull_ReturnsName()
        {
            var model = new ModelWithTableAttribute { Id = 1 };

            var result = model.GetTableName();

            Assert.Equal(nameof(ModelWithTableAttribute), result);
        }

        [Fact]
        public void GetTableAttribute_WithType_TypeIsNull_ReturnsNull()
        {
            Type type = null;

            var result = type.GetTableAttribute();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableAttribute_WithType_TypeOfModelWithoutTableAttribute_ReturnsNull()
        {
            var type = typeof(ModelWithoutTableAttribute);

            var result = type.GetTableAttribute();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableAttribute_WithType_TypeOfModelWithTableAttribute_ReturnsTableAttributeValue()
        {
            var type = typeof(ModelWithTableAttribute);

            var result = type.GetTableAttribute();

            Assert.NotNull(result);
            Assert.Equal(nameof(ModelWithTableAttribute), result.Name);
        }

        [Fact]
        public void GetTableName_WithType_GetTableAttributeReturnsNull_ReturnsNull()
        {
            Type type = null;

            var result = type.GetTableName();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableName_WithType_GetTableAttributeReturnsNotNull_ReturnsName()
        {
            var type = typeof(ModelWithTableAttribute);

            var result = type.GetTableName();

            Assert.Equal(nameof(ModelWithTableAttribute), result);
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
