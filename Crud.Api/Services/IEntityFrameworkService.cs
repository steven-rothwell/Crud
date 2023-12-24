using System.Linq.Expressions;

namespace Crud.Api.Services
{
    public interface IEntityFrameworkService
    {
        IEnumerable<Expression<Func<T, Boolean>>> GetQueryParamFilterExpressions<T>(IDictionary<String, String>? queryParams);
    }
}
