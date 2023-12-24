using System.Linq.Expressions;
using Crud.Api.Constants;

namespace Crud.Api.Services
{
    public class EntityFrameworkService : IEntityFrameworkService
    {
        public IEnumerable<Expression<Func<T, Boolean>>> GetQueryParamFilterExpressions<T>(IDictionary<String, String>? queryParams)
        {
            var expressions = new List<Expression<Func<T, Boolean>>>();
            var type = typeof(T);

            if (queryParams is not null)
            {
                foreach (var queryParam in queryParams)
                {
                    var propertyInfo = type.GetProperties().GetProperty(queryParam.Key, Delimiter.MongoDbChildProperty);
                    string propertyName = propertyInfo!.Name.Replace(Delimiter.QueryParamChildProperty, Delimiter.MongoDbChildProperty);
                    dynamic? propertyValue = queryParam.Value.ChangeType(propertyInfo!.PropertyType);

                    expressions.Add(GetPropertyExpression<T>(propertyName, propertyValue));
                }
            }

            return expressions;
        }

        public Expression<Func<T, Boolean>> GetPropertyExpression<T>(String propertyName, dynamic? propertyValue)
        {
            var parameter = Expression.Parameter(typeof(T), "model");  // "model" is the name of the parameter in the lamda expression. Ex: .Where(model => model.IsActive == true)
            var property = Expression.Property(parameter, propertyName);
            var value = Expression.Constant(propertyValue);
            var equal = Expression.Equal(property, value);

            return Expression.Lambda<Func<T, bool>>(equal, parameter);
        }
    }
}
