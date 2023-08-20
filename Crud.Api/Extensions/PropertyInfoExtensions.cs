using System.Collections;
using System.Reflection;

namespace Crud.Api
{
    public static class PropertyInfoExtensions
    {
        public static PropertyInfo? GetProperty(this PropertyInfo[]? properties, String propertyName, Char childPropertyDelimiter = default, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (properties is null)
                return null;

            int childPropertyDelimiterIndex = -1;
            if (childPropertyDelimiter != default)
            {
                childPropertyDelimiterIndex = propertyName.IndexOf(childPropertyDelimiter, stringComparison);

                if (childPropertyDelimiterIndex == 0)
                    throw new ArgumentException($"{nameof(propertyName)} cannot begin with {childPropertyDelimiter}.");
            }

            if (childPropertyDelimiterIndex == -1)
            {
                return properties.FirstOrDefault(property => property.Name.Equals(propertyName, stringComparison));
            }
            else
            {
                string parentPropertyName = propertyName.Substring(0, childPropertyDelimiterIndex);

                var childPropertyInfo = properties.FirstOrDefault(property => property.Name.Equals(parentPropertyName, stringComparison));
                if (childPropertyInfo is null)
                    return null;

                var childPropertyType = childPropertyInfo.PropertyType;
                string nextPropertyName = propertyName.Substring(childPropertyDelimiterIndex + 1);
                if (typeof(IEnumerable).IsAssignableFrom(childPropertyType) && childPropertyType.IsGenericType)
                {
                    var tType = childPropertyType.GenericTypeArguments.First();
                    return tType.GetProperties().GetProperty(nextPropertyName, childPropertyDelimiter);
                }
                else if (childPropertyType.IsClass && childPropertyType != typeof(string))
                {
                    return childPropertyType.GetProperties().GetProperty(nextPropertyName, childPropertyDelimiter);
                }

                throw new NotSupportedException($"Retrieving child property info from {parentPropertyName} of type {childPropertyType} is unsupported.");
            }
        }

        public static Boolean HasAllPropertyNames(this PropertyInfo[] properties, IEnumerable<String> propertyNames, Char childPropertyDelimiter = default)
        {
            return propertyNames.All(propertyName => properties.GetProperty(propertyName, childPropertyDelimiter) is not null);
        }

        public static Boolean HasPropertyName(this PropertyInfo[] properties, String propertyName, Char childPropertyDelimiter = default)
        {
            return properties.GetProperty(propertyName, childPropertyDelimiter) is not null;
        }
    }
}
