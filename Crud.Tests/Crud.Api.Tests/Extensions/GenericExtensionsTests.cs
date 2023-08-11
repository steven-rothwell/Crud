using System.ComponentModel.DataAnnotations.Schema;
using Crud.Api.Tests.TestingModels;

namespace Crud.Api.Tests.Extensions
{
    public class GenericExtensionsTests
    {
        [Fact]
        public void GetTableAttribute_TIsNull_ReturnsNull()
        {
            Model? model = null;

            var result = model.GetTableAttribute();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableAttribute_ModelWithoutTableAttribute_ReturnsNull()
        {
            var model = new ModelWithoutTableAttribute { Id = 1 };

            var result = model.GetTableAttribute();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableAttribute_ModelWithTableAttribute_ReturnsNameInTableAttribute()
        {
            var model = new ModelWithTableAttribute { Id = 1 };

            var result = model.GetTableAttribute();

            Assert.NotNull(result);
            Assert.Equal(nameof(ModelWithTableAttribute), result.Name);
        }

        [Fact]
        public void GetTableName_ModelIsNull_ReturnsNull()
        {
            Model? model = null;

            var result = model.GetTableName();

            Assert.Null(result);
        }

        [Fact]
        public void GetTableName_TableAttributeIsNotNull_ReturnsNameInTableAttribute()
        {
            var model = new ModelWithTableAttribute { Id = 1 };

            var result = model.GetTableName();

            Assert.Equal(nameof(ModelWithTableAttribute), result);
        }

        [Fact]
        public void GetTableName_TableAttributeIsNull_ReturnsPluralizedNameOfClass()
        {
            var model = new ModelWithoutTableAttribute { Id = 1 };

            var result = model.GetTableName();

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
