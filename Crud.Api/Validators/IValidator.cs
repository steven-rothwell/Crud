using Crud.Api.Models;

namespace Crud.Api.Validators
{
    public interface IValidator
    {
        Task<ValidationResult> ValidateCreateAsync(Object model);
        Task<ValidationResult> ValidateCreateAsync(User user);
        Task<ValidationResult> ValidateReadAsync(Object model, IDictionary<String, String>? queryParams);
        Task<ValidationResult> ValidateReadAsync(User user, IDictionary<String, String>? queryParams);
        Task<ValidationResult> ValidateUpdateAsync(Guid id, Object model);
        Task<ValidationResult> ValidateUpdateAsync(Guid id, User user);
        Task<ValidationResult> ValidatePartialUpdateAsync(Guid id, Object model, IReadOnlyCollection<String> propertiesToBeUpdated);
        Task<ValidationResult> ValidatePartialUpdateAsync(Guid id, User user, IReadOnlyCollection<String> propertiesToBeUpdated);
        Task<ValidationResult> ValidatePartialUpdateAsync(Object model, IDictionary<String, String>? queryParams, IReadOnlyCollection<String> propertiesToBeUpdated);
        Task<ValidationResult> ValidatePartialUpdateAsync(User user, IDictionary<String, String>? queryParams, IReadOnlyCollection<String> propertiesToBeUpdated);
        Task<ValidationResult> ValidateDeleteAsync(Object model, IDictionary<String, String>? queryParams);
        Task<ValidationResult> ValidateDeleteAsync(User user, IDictionary<String, String>? queryParams);
    }
}