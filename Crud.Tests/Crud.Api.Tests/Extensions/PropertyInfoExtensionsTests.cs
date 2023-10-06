using System.Reflection;
using System.Text.Json.Serialization;
using Crud.Api.Constants;

namespace Crud.Api.Tests.Extensions
{
    public class PropertyInfoExtensionsTests
    {
        private const String _parentName = "parentName";
        private const String _childName = "childName";

        [Fact]
        public void GetProperty_PropertiesAreNull_ReturnsNull()
        {
            PropertyInfo[]? properties = null;
            var propertyName = "propertyName";

            var result = properties.GetProperty(propertyName);

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void GetProperty_PropertyNameIsNullOrWhiteSpace_ReturnsNull(String? propertyName)
        {
            var properties = typeof(Parent).GetProperties();

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
        public void GetProperty_ChildPropertyDelimiterNotInPropertyNamePropertyMatchesAlias_ReturnsPropertyInfo()
        {
            var properties = typeof(Parent).GetProperties();
            var propertyName = _parentName;

            var result = properties.GetProperty(propertyName);

            Assert.NotNull(result);
            Assert.Equal(typeof(Parent), result.ReflectedType);
            Assert.Equal(typeof(string), result.PropertyType);
            Assert.Equal(nameof(Parent.Name), result.Name);
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
        public void GetProperty_ChildPropertyDelimiterInPropertyNameParentPropertyIsClassPropertyNameMatchesAliasInChild_ReturnsPropertyInfo()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = $"{nameof(Parent.Child)}{childPropertyDelimiter}{_childName}";

            var result = properties.GetProperty(propertyName, childPropertyDelimiter);

            Assert.NotNull(result);
            Assert.Equal(typeof(Child), result.ReflectedType);
            Assert.Equal(typeof(string), result.PropertyType);
            Assert.Equal(nameof(Child.Name), result.Name);
        }

        [Fact]
        public void GetProperty_ChildPropertyDelimiterInPropertyNameParentPropertyImplementsIEnumerable_ReturnsPropertyInfo()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = $"{nameof(Parent.GrandChildren)}{childPropertyDelimiter}{nameof(Child.Id)}";

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
            Assert.Equal($"Retrieving child property info from {nameof(Parent.Id)} of type {typeof(int)} is unsupported.", exception.Message);
        }

        [Fact]
        public void GetProperty_ChildPropertyDelimiterInPropertyNameParentPropertyIsString_ThrowsNotSupportedException()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = $"{nameof(Parent.Name)}{childPropertyDelimiter}propertyName";

            var action = () => properties.GetProperty(propertyName, childPropertyDelimiter);

            var exception = Assert.Throws<NotSupportedException>(action);
            Assert.Equal($"Retrieving child property info from {nameof(Parent.Name)} of type {typeof(string)} is unsupported.", exception.Message);
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

        [Fact]
        public void HasPropertyName_GetPropertyReturnsNull_ReturnsFalse()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = "ParentPropertyDoesNotExist";

            var result = properties.HasPropertyName(propertyName, childPropertyDelimiter);

            Assert.False(result);
        }

        [Fact]
        public void HasPropertyName_GetPropertyReturnsNotNull_ReturnsTrue()
        {
            var properties = typeof(Parent).GetProperties();
            var childPropertyDelimiter = Delimiter.QueryParamChildProperty;
            var propertyName = nameof(Parent.Id);

            var result = properties.HasPropertyName(propertyName, childPropertyDelimiter);

            Assert.True(result);
        }

        [Fact]
        void MatchesAlias_PropertyInfoIsNull_ReturnsFalse()
        {
            PropertyInfo? propertyInfo = null;
            String? propertyName = "propertyName";

            var result = propertyInfo.MatchesAlias(propertyName);

            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        void MatchesAlias_PropertyNameIsNullOrWhiteSpace_ReturnsFalse(String? propertyName)
        {
            PropertyInfo? propertyInfo = typeof(Parent).GetProperty(nameof(Parent.Name));

            var result = propertyInfo.MatchesAlias(propertyName);

            Assert.False(result);
        }

        [Fact]
        void MatchesAlias_JsonPropertyNameAttributeIsNotDefined_ReturnsFalse()
        {
            PropertyInfo? propertyInfo = typeof(Parent).GetProperty(nameof(Parent.Id));
            String? propertyName = "propertyName";

            var result = propertyInfo.MatchesAlias(propertyName);

            Assert.False(result);
        }

        [Fact]
        void MatchesAlias_JsonPropertyNameAttributeIsDefinedNameDoesNotMatch_ReturnsFalse()
        {
            PropertyInfo? propertyInfo = typeof(Parent).GetProperty(nameof(Parent.Name));
            String? propertyName = "propertyName";

            var result = propertyInfo.MatchesAlias(propertyName);

            Assert.False(result);
        }

        [Fact]
        void MatchesAlias_JsonPropertyNameAttributeIsDefinedNameDoesMatch_ReturnsTrue()
        {
            PropertyInfo? propertyInfo = typeof(Parent).GetProperty(nameof(Parent.Name));
            String? propertyName = _parentName;

            var result = propertyInfo.MatchesAlias(propertyName);

            Assert.True(result);
        }

        private class Parent
        {
            public Int32 Id { get; set; }
            [JsonPropertyName(_parentName)]
            public String? Name { get; set; }
            public Child? Child { get; set; }
            public List<Child>? GrandChildren { get; set; }
        }

        private class Child
        {
            public Int32 Id { get; set; }
            [JsonPropertyName(_childName)]
            public String? Name { get; set; }
        }
    }
}
