using System.Text.Json;
using Crud.Api.QueryModels;

namespace Crud.Api.Preservers
{
    public interface IPreserver
    {
        Task<T> CreateAsync<T>(T model);
        Task<T?> ReadAsync<T>(Guid id);
        Task<IEnumerable<T>> ReadAsync<T>(IDictionary<String, String>? queryParams);
        Task<IEnumerable<T>> QueryReadAsync<T>(Query query);
        Task<Int64> QueryReadCountAsync(Type type, Query query);
        Task<T> UpdateAsync<T>(Guid id, T model);
        Task<T> PartialUpdateAsync<T>(Guid id, IDictionary<String, JsonElement> propertyValues);
        Task<Int64> PartialUpdateAsync<T>(IDictionary<String, String>? queryParams, IDictionary<String, JsonElement> propertyValues);
        Task<Int64> DeleteAsync<T>(Guid id);
        Task<Int64> DeleteAsync<T>(IDictionary<String, String>? queryParams);
        Task<Int64> QueryDeleteAsync(Type type, Query query);
    }
}
