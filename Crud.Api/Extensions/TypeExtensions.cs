using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Crud.Api.Attributes;
using Crud.Api.Enums;
using Humanizer;

namespace Crud.Api
{
    public static class TypeExtensions
    {
        public static TableAttribute? GetTableAttribute(this Type? type)
        {
            if (type is null)
                return null;

            return Attribute.GetCustomAttribute(type, typeof(TableAttribute)) as TableAttribute;
        }

        public static String? GetTableName(this Type? type)
        {
            if (type is null)
                return null;

            var tableAttribute = type.GetTableAttribute();

            if (tableAttribute is not null)
                return tableAttribute.Name;

            return type.GetPluralizedName();
        }

        public static String? GetPluralizedName(this Type? type)
        {
            if (type is null)
                return null;

            return type.Name.Pluralize();
        }

        public static Boolean AllowsCrudOperation(this Type type, CrudOperation crudOperation)
        {
            var attribute = type.GetCustomAttribute<PreventCrudAttribute>();

            if (attribute is not null)
            {
                return attribute.AllowsCrudOperation(crudOperation);
            }

            return true;
        }
    }
}
