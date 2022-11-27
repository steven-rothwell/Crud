using System.Text.Json.Nodes;

namespace Crud.Api.Preservers
{
    public interface IPreserver
    {
        Task<T> CreateAsync<T>(T model);
        Task<T?> ReadAsync<T>(Guid id);
        Task<IEnumerable<T>> ReadAsync<T>(IDictionary<String, String>? queryParams);
        Task<T> UpdateAsync<T>(Guid id, T model);
        Task<T> PartialUpdateAsync<T>(Guid id, IDictionary<String, JsonNode> propertyValues);
        Task<Int64> PartialUpdateAsync<T>(IDictionary<String, String>? queryParams, IDictionary<String, JsonNode> propertyValues);
        Task<Int64> DeleteAsync<T>(Guid id);
        Task<Int64> DeleteAsync<T>(IDictionary<String, String>? queryParams);
    }
}
