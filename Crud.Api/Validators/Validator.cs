using Crud.Api.Constants;
using Crud.Api.Models;

namespace Crud.Api.Validators
{
    public class Validator : IValidator
    {
        public Task<Boolean> ValidateCreateAsync(Object model)
        {
            return Task.FromResult(true);
        }

        public Task<Boolean> ValidateCreateAsync(User user)
        {
            if (user is null)
                return Task.FromResult(false);

            if (user.ExternalId is not null)
                return Task.FromResult(false);

            if (String.IsNullOrWhiteSpace(user.Name))
                return Task.FromResult(false);

            if (user.Age < 0)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<Boolean> ValidateReadAsync(Object model, IDictionary<String, String>? queryParams)
        {
            if (queryParams is null || queryParams.Count == 0)  // Remove to allow returning all.
                return Task.FromResult(false);

            var propertyNames = model.GetType().GetProperties().Select(property => property.Name).ToList();

            if (!model.GetType().GetProperties().HasAllPropertyNames(queryParams.Select(queryParam => queryParam.Key), Delimiter.QueryParamChildProperty))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<Boolean> ValidateReadAsync(User user, IDictionary<String, String>? queryParams)
        {
            return ValidateReadAsync((object)user, queryParams);
        }

        public Task<Boolean> ValidateUpdateAsync(Guid id, Object model)
        {
            return Task.FromResult(true);
        }

        public Task<Boolean> ValidateUpdateAsync(Guid id, User user)
        {
            if (id == Guid.Empty)
                return Task.FromResult(false);

            if (user is null)
                return Task.FromResult(false);

            if (id != user.ExternalId)
                return Task.FromResult(false);

            if (String.IsNullOrWhiteSpace(user.Name))
                return Task.FromResult(false);

            if (user.Age < 0)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<Boolean> ValidatePartialUpdateAsync(Guid id, Object model, IReadOnlyCollection<String> propertiesToBeUpdated)
        {
            if (propertiesToBeUpdated is null || propertiesToBeUpdated.Count == 0)
                return Task.FromResult(false);

            var propertyNames = model.GetType().GetProperties().Select(property => property.Name).ToList();

            if (!model.GetType().GetProperties().HasAllPropertyNames(propertiesToBeUpdated))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public async Task<Boolean> ValidatePartialUpdateAsync(Guid id, User user, IReadOnlyCollection<String> propertiesToBeUpdated)
        {
            if (!await ValidatePartialUpdateAsync(id, (object)user, propertiesToBeUpdated))
                return false;

            if (user is null)
                return false;

            if (WillBeUpdated(nameof(user.ExternalId), propertiesToBeUpdated))  // Prevent updating this property.
                return false;

            if (WillBeUpdated(nameof(user.Name), propertiesToBeUpdated) && String.IsNullOrWhiteSpace(user.Name))
                return false;

            if (WillBeUpdated(nameof(user.Age), propertiesToBeUpdated) && user.Age < 0)
                return false;

            return true;
        }

        private Boolean WillBeUpdated(String propertyName, IEnumerable<String> propertiesToBeUpdated)
        {
            return propertiesToBeUpdated?.Contains(propertyName, StringComparer.OrdinalIgnoreCase) ?? false;
        }
    }
}
