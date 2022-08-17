using System.Reflection;

namespace Crud.Api
{
    public static class PropertyInfoExtensions
    {


        public static PropertyInfo? GetProperty(this PropertyInfo[] properties, String propertyName, Char childPropertyDelimiter)
        {
            int childPropertyIndex = propertyName.IndexOf(childPropertyDelimiter);
            if (childPropertyIndex == 0)
                throw new ArgumentException($"{nameof(propertyName)} cannot begin with {childPropertyDelimiter}.");

            if (childPropertyIndex == -1)
            {
                return properties.FirstOrDefault(property => property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                string childPropertyName = propertyName.Substring(0, childPropertyIndex);

                var childPropertyInfo = properties.FirstOrDefault(property => property.Name.Equals(childPropertyName, StringComparison.OrdinalIgnoreCase));
                if (childPropertyInfo is null)
                    return null;

                var childPropertyType = childPropertyInfo.PropertyType;

                if (childPropertyType.IsClass)
                {
                    string nextPropertyName = propertyName.Substring(childPropertyIndex + 1);
                    return childPropertyType.GetProperties().GetProperty(nextPropertyName, childPropertyDelimiter);
                }

                throw new NotSupportedException($"Child property {childPropertyName} of type {childPropertyType} is unsupported.");
            }
        }

        public static Boolean HasAllPropertyNames(this PropertyInfo[] properties, IEnumerable<String> propertyNames, Char childPropertyDelimiter)
        {
            return propertyNames.All(propertyName => properties.GetProperty(propertyName, childPropertyDelimiter) is not null);
        }
    }
}
