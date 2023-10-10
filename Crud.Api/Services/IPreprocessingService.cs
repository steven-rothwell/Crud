using Crud.Api.QueryModels;
using Crud.Api.Services.Models;

namespace Crud.Api.Services
{
    public interface IPreprocessingService
    {
        Task<MessageResult> PreprocessCreateAsync(Object model);
        Task<MessageResult> PreprocessReadAsync(Object model, Guid id);
        Task<MessageResult> PreprocessReadAsync(Object model, IDictionary<String, String>? queryParams);
        Task<MessageResult> PreprocessReadAsync(Object model, Query query);
    }
}
