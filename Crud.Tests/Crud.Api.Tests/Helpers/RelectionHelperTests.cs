using Crud.Api.Helpers;

namespace Crud.Api.Tests.Helpers
{
    public class RelectionHelperTests
    {
        [Fact]
        public void GetGenericMethod_MethodIsNull_ThrowsException()
        {
            var t = typeof(SomeOtherClass);
            var classOfMethod = typeof(ClassWithGenericMethod);
            var methodName = "ThisMethodDoesNotExist";
            var parameterTypes = new Type[] { typeof(string), typeof(Guid) };

            var action = () => ReflectionHelper.GetGenericMethod(t, classOfMethod, methodName, parameterTypes);

            var exception = Assert.Throws<Exception>(action);
            Assert.Equal($"Unable to get method. {methodName} does not exist.", exception.Message);
        }

        [Fact]
        public void GetGenericMethod_MethodIsNotNull_ReturnsMethodInfo()
        {
            var t = typeof(SomeOtherClass);
            var classOfMethod = typeof(ClassWithGenericMethod);
            var methodName = nameof(ClassWithGenericMethod.SomeGenericMethod);
            var parameterTypes = new Type[] { typeof(string), typeof(Guid) };

            var result = ReflectionHelper.GetGenericMethod(t, classOfMethod, methodName, parameterTypes);

            Assert.NotNull(result);
            Assert.Equal(t, result.ReturnType);
            Assert.Equal(parameterTypes.Length, result.GetParameters().Length);
        }

        private class ClassWithGenericMethod
        {
            public T SomeGenericMethod<T>() where T : new()
            {
                return new T();
            }

            public T SomeGenericMethod<T>(String someStringParameter, Guid someGuidParameter) where T : new()
            {
                return new T();
            }
        }

        private class SomeOtherClass
        {
            public Int32 Id { get; set; }
        }
    }
}
