using System.Reflection;
using Crud.Api.Constants;

namespace Crud.Api.Tests.Extensions
{
    public class PropertyInfoExtensionsTests
    {
        [Fact]
        public void GetProperty_PropertiesAreNull_ReturnsNull()
        {
            PropertyInfo[] properties = null;
            var propertyName = "propertyName";

            var result = properties.GetProperty(propertyName);

            Assert.Null(result);
        }

        [Fact]
        public void GetProperty_PropertyNameStartsWithNotDefaultChildPropertyDelimiter_ThrowsArgumentException()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = $"{childPropertyDelimiter}propertyName";

            var action = () => properties.GetProperty(propertyName, childPropertyDelimiter);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal($"{nameof(propertyName)} cannot begin with {childPropertyDelimiter}.", exception.Message);
        }

        [Fact]
        public void GetProperty_ChildPropertyDelimiterNotInPropertyNamePropertyExists_ReturnsPropertyInfo()
        {
            var properties = typeof(Parent).GetProperties();
            var propertyName = nameof(Parent.Id);

            var result = properties.GetProperty(propertyName);

            Assert.NotNull(result);
            Assert.Equal(typeof(Parent), result.ReflectedType);
            Assert.Equal(typeof(int), result.PropertyType);
            Assert.Equal(nameof(Parent.Id), result.Name);
        }

        [Fact]
        public void GetProperty_ChildPropertyDelimiterNotInPropertyNamePropertyDoesNotExist_ReturnsNull()
        {
            var properties = typeof(Parent).GetProperties();
            var propertyName = "PropertyDoesNotExist";

            var result = properties.GetProperty(propertyName);

            Assert.Null(result);
        }

        [Fact]
        public void GetProperty_ChildPropertyDelimiterInPropertyNamePropertyNameDoesNotExistInParent_ReturnsNull()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = $"ParentPropertyDoesNotExist{childPropertyDelimiter}{nameof(Child.Id)}";

            var result = properties.GetProperty(propertyName, childPropertyDelimiter);

            Assert.Null(result);
        }

        [Fact]
        public void GetProperty_ChildPropertyDelimiterInPropertyNameParentPropertyIsClassPropertyNameExistsInChild_ReturnsPropertyInfo()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = $"{nameof(Parent.Child)}{childPropertyDelimiter}{nameof(Child.Id)}";

            var result = properties.GetProperty(propertyName, childPropertyDelimiter);

            Assert.NotNull(result);
            Assert.Equal(typeof(Child), result.ReflectedType);
            Assert.Equal(typeof(int), result.PropertyType);
            Assert.Equal(nameof(Child.Id), result.Name);
        }

        [Fact]
        public void GetProperty_ChildPropertyDelimiterInPropertyNameParentPropertyIsClassPropertyNameDoesNotExistsInChild_ReturnsNull()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = $"{nameof(Parent.Child)}{childPropertyDelimiter}ChildPropertyDoesNotExist";

            var result = properties.GetProperty(propertyName, childPropertyDelimiter);

            Assert.Null(result);
        }

        [Fact]
        public void GetProperty_ChildPropertyDelimiterInPropertyNameParentPropertyIsNotClass_ThrowsNotSupportedException()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = $"{nameof(Parent.Id)}{childPropertyDelimiter}propertyName";

            var action = () => properties.GetProperty(propertyName, childPropertyDelimiter);

            var exception = Assert.Throws<NotSupportedException>(action);
            Assert.Equal($"Child property {nameof(Parent.Id)} of type {typeof(int)} is unsupported.", exception.Message);
        }

        [Fact]
        public void GetProperty_ChildPropertyDelimiterInPropertyNameParentPropertyIsString_ThrowsNotSupportedException()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = $"{nameof(Parent.Name)}{childPropertyDelimiter}propertyName";

            var action = () => properties.GetProperty(propertyName, childPropertyDelimiter);

            var exception = Assert.Throws<NotSupportedException>(action);
            Assert.Equal($"Child property {nameof(Parent.Name)} of type {typeof(string)} is unsupported.", exception.Message);
        }

        [Fact]
        public void HasAllPropertyNames_GetPropertyNeverReturnsNull_ReturnsTrue()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyNames = new List<string>
            {
                nameof(Parent.Id),
                $"{nameof(Parent.Child)}{childPropertyDelimiter}{nameof(Child.Id)}"
            };

            var result = properties.HasAllPropertyNames(propertyNames, childPropertyDelimiter);

            Assert.True(result);
        }

        [Fact]
        public void HasAllPropertyNames_GetPropertyReturnsAtLeastOneNull_ReturnsFalse()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyNames = new List<string>
            {
                nameof(Parent.Id),
                $"{nameof(Parent.Child)}{childPropertyDelimiter}{nameof(Child.Id)}",
                "ParentPropertyDoesNotExist"
            };

            var result = properties.HasAllPropertyNames(propertyNames, childPropertyDelimiter);

            Assert.False(result);
        }

        private class Parent
        {
            public Int32 Id { get; set; }
            public String Name { get; set; }
            public Child Child { get; set; }
        }

        private class Child
        {
            public Int32 Id { get; set; }
        }
    }
}
