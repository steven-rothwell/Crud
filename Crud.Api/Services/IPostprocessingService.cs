using Crud.Api.Services.Models;

namespace Crud.Api.Services
{
    public interface IPostprocessingService
    {
        Task<MessageResult> PostprocessCreateAsync(Object model);
        Task<MessageResult> PostprocessReadAsync(Object model, Guid id);
        Task<MessageResult> PostprocessReadAsync(IEnumerable<Object> models, IDictionary<String, String>? queryParams);
    }
}
