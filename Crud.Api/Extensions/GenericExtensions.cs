using System.ComponentModel.DataAnnotations.Schema;

namespace Crud.Api
{
    public static class GenericExtensions
    {
        public static TableAttribute? GetTableAttribute<T>(this T t)
        {
            if (t is null)
                return null;

            return Attribute.GetCustomAttribute(t.GetType(), typeof(TableAttribute)) as TableAttribute;
        }

        public static String? GetTableName<T>(this T t)
        {
            if (t is null)
                return null;

            var tableAttribute = t.GetTableAttribute();

            if (tableAttribute is not null)
                return tableAttribute.Name;

            return t.GetType().GetPluralizedName();
        }
    }
}
