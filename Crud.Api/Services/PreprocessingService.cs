using System.Text.Json;
using Crud.Api.QueryModels;
using Crud.Api.Services.Models;

namespace Crud.Api.Services
{
    public class PreprocessingService : IPreprocessingService
    {
        public PreprocessingService()
        {

        }

        public Task<MessageResult> PreprocessCreateAsync(Object model)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PreprocessReadAsync(Object model, Guid id)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PreprocessReadAsync(Object model, IDictionary<String, String>? queryParams)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PreprocessReadAsync(Object model, Query query)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PreprocessReadCountAsync(Object model, Query query)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PreprocessUpdateAsync(Object model, Guid id)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PreprocessPartialUpdateAsync(Object model, Guid id, IDictionary<String, JsonElement> propertyValues)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PreprocessPartialUpdateAsync(Object model, IDictionary<String, String>? queryParams, IDictionary<String, JsonElement> propertyValues)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PreprocessDeleteAsync(Object model, Guid id)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PreprocessDeleteAsync(Object model, IDictionary<String, String>? queryParams)
        {
            return Task.FromResult(new MessageResult(true));
        }
    }
}
