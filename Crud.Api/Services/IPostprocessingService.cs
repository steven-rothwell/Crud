using Crud.Api.QueryModels;
using Crud.Api.Services.Models;

namespace Crud.Api.Services
{
    public interface IPostprocessingService
    {
        Task<MessageResult> PostprocessCreateAsync(Object createdModel);
        Task<MessageResult> PostprocessReadAsync(Object model, Guid id);
        Task<MessageResult> PostprocessReadAsync(IEnumerable<Object> models, IDictionary<String, String>? queryParams);
        Task<MessageResult> PostprocessReadAsync(IEnumerable<Object> models, Query query);
        Task<MessageResult> PostprocessReadCountAsync(Object model, Query query, Int64 count);
        Task<MessageResult> PostprocessUpdateAsync(Object updatedModel, Guid id);
    }
}
