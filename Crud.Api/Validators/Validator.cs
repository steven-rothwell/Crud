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

            return Task.FromResult(true);
        }

        public Task<Boolean> ValidateReadAsync(Object model, IDictionary<String, String>? queryParams)
        {
            if (queryParams is null)  // Remove to allow returning all.
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

        public Task<Boolean> ValidateUpdateAsync(Object model)
        {
            return Task.FromResult(true);
        }

        public Task<Boolean> ValidateUpdateAsync(User user)
        {
            if (user is null)
                return Task.FromResult(false);

            if (user.ExternalId is not null)
                return Task.FromResult(false);

            if (String.IsNullOrWhiteSpace(user.Name))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
    }
}
