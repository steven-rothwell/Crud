using System.ComponentModel.DataAnnotations.Schema;

namespace Crud.Api
{
    public static class GenericExtensions
    {
        public static TableAttribute? GetTableAttribute<T>(this T t)
        {
            return Attribute.GetCustomAttribute(t!.GetType(), typeof(TableAttribute)) as TableAttribute;
        }

        public static String? GetTableName<T>(this T t)
        {
            return t.GetTableAttribute()?.Name;
        }

        public static TableAttribute? GetTableAttribute(this Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(TableAttribute)) as TableAttribute;
        }

        public static String? GetTableName(this Type type)
        {
            return type.GetTableAttribute()?.Name;
        }
    }
}