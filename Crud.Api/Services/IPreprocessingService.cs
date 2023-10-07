using Crud.Api.Services.Models;

namespace Crud.Api.Services
{
    public interface IPreprocessingService
    {
        Task<MessageResult> PreprocessCreateAsync(Object model);
    }
}
