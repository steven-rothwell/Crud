using System.Text;
using Humanizer;

namespace Crud.Api
{
    public static class StringExtensions
    {
        public static dynamic? ChangeType(this String? value, Type? type)
        {
            if (value is null)
                return null;

            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            if (type == typeof(Guid)) return Guid.Parse(value);
            else return Convert.ChangeType(value, type!);
        }

        public static String Pascalize(this String? value, Char delimiter)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var subValues = value.Split(delimiter);

            if (subValues.Length == 1)
                return value.Pascalize();

            var valueBuilder = new StringBuilder();
            foreach (var subValue in subValues)
            {
                valueBuilder.Append(String.Concat(delimiter, subValue.Pascalize()));
            }

            return valueBuilder.ToString(1, valueBuilder.Length - 1);
        }

        public static String Camelize(this String? value, Char delimiter)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var subValues = value.Split(delimiter);

            if (subValues.Length == 1)
                return value.Camelize();

            var valueBuilder = new StringBuilder();
            foreach (var subValue in subValues)
            {
                valueBuilder.Append(String.Concat(delimiter, subValue.Camelize()));
            }

            return valueBuilder.ToString(1, valueBuilder.Length - 1);
        }

        public static String GetValueAfterFirstDelimiter(this String? value, Char delimiter)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var indexOfFirstDelimiter = value.IndexOf(delimiter);

            if (indexOfFirstDelimiter > -1)
            {
                return value.Substring(indexOfFirstDelimiter + 1);
            }

            return value;
        }

        public static String GetValueAfterLastDelimiter(this String? value, Char delimiter)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var indexOfLastDelimiter = value.LastIndexOf(delimiter);

            if (indexOfLastDelimiter > -1)
            {
                return value.Substring(indexOfLastDelimiter + 1);
            }

            return value;
        }
    }
}
