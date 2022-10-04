using Crud.Api.Constants;
using Humanizer;

namespace Crud.Api.Functions
{
    public static class TypeFunc
    {
        public static Type? GetType(String @namespace, String typeName)
        {
            if (String.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentException($"{nameof(@namespace)} cannot be null or whitespace.");

            if (String.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException($"{nameof(typeName)} cannot be null or whitespace.");

            return Type.GetType($"{@namespace}.{typeName.Singularize().Pascalize()}");
        }

        public static Type? GetModelType(String typeName)
        {
            return GetType(Namespace.Models, typeName);
        }
    }
}
