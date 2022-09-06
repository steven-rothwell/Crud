using Crud.Api.Models;

namespace Crud.Api.Validators
{
    public interface IValidator
    {
        Task<Boolean> ValidateCreateAsync(Object model);
        Task<Boolean> ValidateCreateAsync(User user);
        Task<Boolean> ValidateReadAsync(Object model, IDictionary<String, String>? queryParams);
        Task<Boolean> ValidateReadAsync(User user, IDictionary<String, String>? queryParams);
        Task<Boolean> ValidateUpdateAsync(Guid id, Object model);
        Task<Boolean> ValidateUpdateAsync(Guid id, User user);
        Task<Boolean> ValidatePartialUpdateAsync(Guid id, Object model, IReadOnlyCollection<String> propertiesToBeUpdated);
        Task<Boolean> ValidatePartialUpdateAsync(Guid id, User user, IReadOnlyCollection<String> propertiesToBeUpdated);
    }
}