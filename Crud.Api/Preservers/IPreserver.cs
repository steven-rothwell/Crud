namespace Crud.Api.Preservers
{
    public interface IPreserver
    {
        Task<T> CreateAsync<T>(T model);
        Task<T> ReadAsync<T>(Guid id);
        Task<IEnumerable<T>> ReadAsync<T>(IDictionary<String, String>? queryParams);
        Task<T> UpdateAsync<T>(Guid id, IDictionary<String, String> propertyValues);
    }
}
