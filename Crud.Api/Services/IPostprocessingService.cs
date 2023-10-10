using Crud.Api.Services.Models;

namespace Crud.Api.Services
{
    public interface IPostprocessingService
    {
        Task<MessageResult> PostprocessCreateAsync(Object model);
        Task<MessageResult> PostprocessReadAsync(Object model, Guid id);
    }
}
