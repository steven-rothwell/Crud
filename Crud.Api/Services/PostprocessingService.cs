using Crud.Api.Services.Models;

namespace Crud.Api.Services
{
    public class PostprocessingService : IPostprocessingService
    {
        public PostprocessingService()
        {

        }

        public Task<MessageResult> PostprocessCreateAsync(Object model)
        {
            return Task.FromResult(new MessageResult(true));
        }

        public Task<MessageResult> PostprocessReadAsync(Object model, Guid id)
        {
            return Task.FromResult(new MessageResult(true));
        }
    }
}
