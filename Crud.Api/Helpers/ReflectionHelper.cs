using System.Reflection;

namespace Crud.Api.Helpers
{
    public static class ReflectionHelper
    {
        public static MethodInfo GetGenericMethod(Type t, Type classOfMethod, String methodName, Type[] parameterTypes)
        {
            var method = classOfMethod.GetMethod(methodName, parameterTypes);
            if (method is null)
                throw new Exception($"Unable to get method. {methodName} does not exist.");

            return method.MakeGenericMethod(t);
        }
    }
}
