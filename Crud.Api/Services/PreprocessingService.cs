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
    }
}
