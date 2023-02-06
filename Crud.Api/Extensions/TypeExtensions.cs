using System.ComponentModel.DataAnnotations.Schema;
using Humanizer;

namespace Crud.Api
{
    public static class TypeExtensions
    {
        public static TableAttribute? GetTableAttribute(this Type type)
        {
            if (type is null)
                return null;

            return Attribute.GetCustomAttribute(type, typeof(TableAttribute)) as TableAttribute;
        }

        public static String? GetTableName(this Type type)
        {
            if (type is null)
                return null;

            var tableAttribute = type.GetTableAttribute();

            if (tableAttribute is not null)
                return tableAttribute.Name;

            return type.GetPluralizedName();
        }

        public static String? GetPluralizedName(this Type type)
        {
            if (type is null)
                return null;

            return type.Name.Pluralize();
        }
    }
}
