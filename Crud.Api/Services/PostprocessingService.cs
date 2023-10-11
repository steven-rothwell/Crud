using System.Text.Json;
using Crud.Api.QueryModels;
using Crud.Api.Services.Models;

namespace Crud.Api.Services
{
    public class PostprocessingService : IPostprocessingService
    {
        public PostprocessingService()
        {

        }

        public Task<MessageResult> PostprocessCreateAsync(Object createdModel)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PostprocessReadAsync(Object model, Guid id)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PostprocessReadAsync(IEnumerable<Object> models, IDictionary<String, String>? queryParams)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PostprocessReadAsync(IEnumerable<Object> models, Query query)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PostprocessReadCountAsync(Object model, Query query, Int64 count)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PostprocessUpdateAsync(Object updatedModel, Guid id)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PostprocessPartialUpdateAsync(Object updatedModel, Guid id, IDictionary<String, JsonElement> propertyValues)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PostprocessPartialUpdateAsync(Object model, IDictionary<String, String>? queryParams, IDictionary<String, JsonElement> propertyValues, Int64 updatedCount)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PostprocessDeleteAsync(Object model, Guid id, Int64 deletedCount)
        {
            return Task.FromResult(new MessageResult(true));
        }
    }
}
