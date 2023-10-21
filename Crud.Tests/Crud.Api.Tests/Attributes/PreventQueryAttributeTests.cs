using Crud.Api.Attributes;
using Crud.Api.QueryModels;

namespace Crud.Api.Tests.Extensions
{
    public class PreventQueryAttributeTests
    {
        [Fact]
        public void AllowsOperator_PreventedOperatorsCountIsZero_ReturnsFalse()
        {
            var preventQueryAttribute = new PreventQueryAttribute();
            var @operator = Operator.Contains;

            var result = preventQueryAttribute.AllowsOperator(@operator);

            Assert.False(result);
        }

        [Fact]
        public void AllowsOperator_PreventedOperatorsContainsOperator_ReturnsFalse()
        {
            var preventQueryAttribute = new PreventQueryAttribute(Operator.Contains);
            var @operator = Operator.Contains;

            var result = preventQueryAttribute.AllowsOperator(@operator);

            Assert.False(result);
        }

        [Fact]
        public void AllowsOperator_PreventedOperatorsDoesNotContainOperator_ReturnsTrue()
        {
            var preventQueryAttribute = new PreventQueryAttribute(Operator.All);
            var @operator = Operator.Contains;

            var result = preventQueryAttribute.AllowsOperator(@operator);

            Assert.True(result);
        }
    }
}