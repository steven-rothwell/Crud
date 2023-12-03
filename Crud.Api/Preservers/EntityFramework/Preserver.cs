using System.Text.Json;
using Crud.Api.Models;
using Crud.Api.QueryModels;

namespace Crud.Api.Preservers.EntityFramework
{
    public class Preserver : IPreserver
    {
        public Task<T> CreateAsync<T>(T model)
        {
            throw new NotImplementedException();

            if (model is null)
                throw new Exception("Cannot create because model is null.");

            if (model is IExternalEntity entity && !entity.ExternalId.HasValue)
            {
                entity.ExternalId = Guid.NewGuid();
            }


        }

        public Task<Int64> DeleteAsync<T>(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> DeleteAsync<T>(IDictionary<String, String>? queryParams)
        {
            throw new NotImplementedException();
        }

        public Task<T?> PartialUpdateAsync<T>(Guid id, IDictionary<String, JsonElement> propertyValues)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> PartialUpdateAsync<T>(IDictionary<String, String>? queryParams, IDictionary<String, JsonElement> propertyValues)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> QueryDeleteAsync(Type type, Query query)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryReadAsync<T>(Query query)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> QueryReadCountAsync(Type type, Query query)
        {
            throw new NotImplementedException();
        }

        public Task<T?> ReadAsync<T>(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> ReadAsync<T>(IDictionary<String, String>? queryParams)
        {
            throw new NotImplementedException();
        }

        public Task<T?> UpdateAsync<T>(T model, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
