using Crud.Api.Models;
using Crud.Api.QueryModels;

namespace Crud.Api.Validators
{
    public interface IValidator
    {
        Task<ValidationResult> ValidateCreateAsync(Object model);
        Task<ValidationResult> ValidateCreateAsync(User user);
        Task<ValidationResult> ValidateReadAsync(Object model, IDictionary<String, String>? queryParams);
        Task<ValidationResult> ValidateReadAsync(User user, IDictionary<String, String>? queryParams);
        Task<ValidationResult> ValidateUpdateAsync(Object model, Guid id);
        Task<ValidationResult> ValidateUpdateAsync(User user, Guid id);
        Task<ValidationResult> ValidatePartialUpdateAsync(Object model, Guid id, IReadOnlyCollection<String>? propertiesToBeUpdated);
        Task<ValidationResult> ValidatePartialUpdateAsync(User user, Guid id, IReadOnlyCollection<String>? propertiesToBeUpdated);
        Task<ValidationResult> ValidatePartialUpdateAsync(Object model, IDictionary<String, String>? queryParams, IReadOnlyCollection<String>? propertiesToBeUpdated);
        Task<ValidationResult> ValidatePartialUpdateAsync(User user, IDictionary<String, String>? queryParams, IReadOnlyCollection<String>? propertiesToBeUpdated);
        Task<ValidationResult> ValidateDeleteAsync(Object model, IDictionary<String, String>? queryParams);
        Task<ValidationResult> ValidateDeleteAsync(User user, IDictionary<String, String>? queryParams);
        ValidationResult ValidateQuery(Object model, Query query);
    }
}