using Crud.Api.Constants;
using Crud.Api.Models;
using Crud.Api.Preservers;

namespace Crud.Api.Validators
{
    public class Validator : IValidator
    {
        private readonly IPreserver _preserver;

        public Validator(IPreserver preserver)
        {
            _preserver = preserver;
        }

        public Task<ValidationResult> ValidateCreateAsync(Object model)
        {
            var validationResult = model.ValidateDataAnnotations();
            if (!validationResult.IsValid)
                return Task.FromResult(validationResult);

            return Task.FromResult(new ValidationResult(true));
        }

        public async Task<ValidationResult> ValidateCreateAsync(User user)
        {
            if (user is null)
                return new ValidationResult(false, $"{nameof(User)} cannot be null.");

            var objectValidationResult = await ValidateCreateAsync((object)user);
            if (!objectValidationResult.IsValid)
                return objectValidationResult;

            if (user.ExternalId is not null)
                return new ValidationResult(false, $"{nameof(user.ExternalId)} cannot be set on create.");

            if (String.IsNullOrWhiteSpace(user.Name))
                return new ValidationResult(false, $"{nameof(user.Name)} cannot be empty.");

            var existingUsers = await _preserver.ReadAsync<User>(new Dictionary<string, string> { { nameof(user.Name), user.Name } });
            if (existingUsers.Any())
                return new ValidationResult(false, $"A {nameof(User)} with the {nameof(user.Name)}: '{user.Name}' already exists.");

            if (user.Age < 0)
                return new ValidationResult(false, $"{nameof(user.Age)} cannot be less than 0.");

            return new ValidationResult(true);
        }

        public Task<ValidationResult> ValidateReadAsync(Object model, IDictionary<String, String>? queryParams)
        {
            if (queryParams is null || queryParams.Count == 0)  // Remove to allow returning all.
                return Task.FromResult(new ValidationResult(false, "Filter cannot be empty."));

            if (!model.GetType().GetProperties().HasAllPropertyNames(queryParams.Select(queryParam => queryParam.Key), Delimiter.QueryParamChildProperty))
                return Task.FromResult(new ValidationResult(false, "Filter cannot contain properties that the model does not have."));

            return Task.FromResult(new ValidationResult(true));
        }

        public Task<ValidationResult> ValidateReadAsync(User user, IDictionary<String, String>? queryParams)
        {
            // The user version of this method and call to object version is not necessary.
            // This is only here to show how to override in case more user validation was necessary.
            return ValidateReadAsync((object)user, queryParams);
        }

        public Task<ValidationResult> ValidateUpdateAsync(Guid id, Object model)
        {
            var validationResult = model.ValidateDataAnnotations();
            if (!validationResult.IsValid)
                return Task.FromResult(validationResult);

            return Task.FromResult(new ValidationResult(true));
        }

        public async Task<ValidationResult> ValidateUpdateAsync(Guid id, User user)
        {
            var objectValidationResult = await ValidateUpdateAsync(id, (object)user);
            if (!objectValidationResult.IsValid)
                return objectValidationResult;

            if (id == Guid.Empty)
                return new ValidationResult(false, "Id cannot be empty.");

            if (user is null)
                return new ValidationResult(false, $"{nameof(User)} cannot be null.");

            if (id != user.ExternalId)
                return new ValidationResult(false, $"{nameof(user.ExternalId)} cannot be altered.");

            if (String.IsNullOrWhiteSpace(user.Name))
                return new ValidationResult(false, $"{nameof(user.Name)} cannot be empty.");

            if (user.Age < 0)
                return new ValidationResult(false, $"{nameof(user.Age)} cannot be less than zero.");

            return new ValidationResult(true);
        }

        public Task<ValidationResult> ValidatePartialUpdateAsync(Guid id, Object model, IReadOnlyCollection<String>? propertiesToBeUpdated)
        {
            if (propertiesToBeUpdated is null || propertiesToBeUpdated.Count == 0)
                return Task.FromResult(new ValidationResult(false, "Updated properties cannot be empty."));

            if (!model.GetType().GetProperties().HasAllPropertyNames(propertiesToBeUpdated))
                return Task.FromResult(new ValidationResult(false, "Updated properties cannot contain properties that the model does not have."));

            var validationResult = model.ValidateDataAnnotations(true, propertiesToBeUpdated);
            if (!validationResult.IsValid)
                return Task.FromResult(validationResult);

            return Task.FromResult(new ValidationResult(true));
        }

        public async Task<ValidationResult> ValidatePartialUpdateAsync(Guid id, User user, IReadOnlyCollection<String>? propertiesToBeUpdated)
        {
            var objectValidationResult = await ValidatePartialUpdateAsync(id, (object)user, propertiesToBeUpdated);
            if (!objectValidationResult.IsValid)
                return objectValidationResult;

            if (WillBeUpdated(nameof(user.ExternalId), propertiesToBeUpdated))  // Prevents updating this property.
                return new ValidationResult(false, $"{nameof(user.ExternalId)} cannot be altered.");

            if (WillBeUpdated(nameof(user.Name), propertiesToBeUpdated) && String.IsNullOrWhiteSpace(user.Name))
                return new ValidationResult(false, $"{nameof(user.Name)} cannot be empty.");

            if (WillBeUpdated(nameof(user.Age), propertiesToBeUpdated) && user.Age < 0)
                return new ValidationResult(false, $"{nameof(user.Age)} cannot be less than zero.");

            return new ValidationResult(true);
        }

        public Task<ValidationResult> ValidatePartialUpdateAsync(Object model, IDictionary<String, String>? queryParams, IReadOnlyCollection<String>? propertiesToBeUpdated)
        {
            if (queryParams is null || queryParams.Count == 0)  // Remove to allow returning all.
                return Task.FromResult(new ValidationResult(false, "Filter cannot be empty."));

            if (!model.GetType().GetProperties().HasAllPropertyNames(queryParams.Select(queryParam => queryParam.Key), Delimiter.QueryParamChildProperty))
                return Task.FromResult(new ValidationResult(false, "Filter cannot contain properties that the model does not have."));

            if (propertiesToBeUpdated is null || propertiesToBeUpdated.Count == 0)
                return Task.FromResult(new ValidationResult(false, "Updated properties cannot be empty."));

            if (!model.GetType().GetProperties().HasAllPropertyNames(propertiesToBeUpdated))
                return Task.FromResult(new ValidationResult(false, "Updated properties cannot contain properties that the model does not have."));

            var validationResult = model.ValidateDataAnnotations(true, propertiesToBeUpdated);
            if (!validationResult.IsValid)
                return Task.FromResult(validationResult);

            return Task.FromResult(new ValidationResult(true));
        }

        public async Task<ValidationResult> ValidatePartialUpdateAsync(User user, IDictionary<String, String>? queryParams, IReadOnlyCollection<String>? propertiesToBeUpdated)
        {
            var objectValidationResult = await ValidatePartialUpdateAsync((object)user, queryParams, propertiesToBeUpdated);
            if (!objectValidationResult.IsValid)
                return objectValidationResult;

            if (user is null)
                return new ValidationResult(false, $"{nameof(User)} cannot be null.");

            if (WillBeUpdated(nameof(user.ExternalId), propertiesToBeUpdated))  // Prevents updating this property.
                return new ValidationResult(false, $"{nameof(user.ExternalId)} cannot be altered.");

            if (WillBeUpdated(nameof(user.Name), propertiesToBeUpdated) && String.IsNullOrWhiteSpace(user.Name))
                return new ValidationResult(false, $"{nameof(user.Name)} cannot be empty.");

            if (WillBeUpdated(nameof(user.Age), propertiesToBeUpdated) && user.Age < 0)
                return new ValidationResult(false, $"{nameof(user.Age)} cannot be less than zero.");

            return new ValidationResult(true);
        }

        public Task<ValidationResult> ValidateDeleteAsync(Object model, IDictionary<String, String>? queryParams)
        {
            if (queryParams is null || queryParams.Count == 0)  // Remove to allow returning all.
                return Task.FromResult(new ValidationResult(false, "Filter cannot be empty."));

            if (!model.GetType().GetProperties().HasAllPropertyNames(queryParams.Select(queryParam => queryParam.Key), Delimiter.QueryParamChildProperty))
                return Task.FromResult(new ValidationResult(false, "Filter cannot contain properties that the model does not have."));

            return Task.FromResult(new ValidationResult(true));
        }

        public Task<ValidationResult> ValidateDeleteAsync(User user, IDictionary<String, String>? queryParams)
        {
            // The user version of this method and call to object version is not necessary.
            // This is only here to show how to override in case more user validation was necessary.
            return ValidateDeleteAsync((object)user, queryParams);
        }

        private Boolean WillBeUpdated(String propertyName, IEnumerable<String>? propertiesToBeUpdated)
        {
            return propertiesToBeUpdated?.Contains(propertyName, StringComparer.OrdinalIgnoreCase) ?? false;
        }
    }
}
