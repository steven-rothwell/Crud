using System.Reflection;
using Crud.Api.Attributes;
using Crud.Api.Constants;
using Crud.Api.Models;
using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.QueryModels;
using Crud.Api.Results;
using Microsoft.Extensions.Options;

namespace Crud.Api.Validators
{
    public class Validator : IValidator
    {
        private readonly IPreserver _preserver;
        private readonly ApplicationOptions _applicationOptions;

        public Validator(IPreserver preserver, IOptions<ApplicationOptions> applicationOptions)
        {
            _preserver = preserver;
            _applicationOptions = applicationOptions.Value;
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
            if (existingUsers is not null && existingUsers.Any())
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

        public Task<ValidationResult> ValidateUpdateAsync(Object model, Guid id)
        {
            var validationResult = model.ValidateDataAnnotations();
            if (!validationResult.IsValid)
                return Task.FromResult(validationResult);

            return Task.FromResult(new ValidationResult(true));
        }

        public async Task<ValidationResult> ValidateUpdateAsync(User user, Guid id)
        {
            var objectValidationResult = await ValidateUpdateAsync((object)user, id);
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

        public Task<ValidationResult> ValidatePartialUpdateAsync(Object model, Guid id, IReadOnlyCollection<String>? propertiesToBeUpdated)
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

        public async Task<ValidationResult> ValidatePartialUpdateAsync(User user, Guid id, IReadOnlyCollection<String>? propertiesToBeUpdated)
        {
            var objectValidationResult = await ValidatePartialUpdateAsync((object)user, id, propertiesToBeUpdated);
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

        public ValidationResult ValidateQuery(Object model, Query query)
        {
            var includesIsPopulated = (query.Includes is not null && query.Includes.Count > 0);
            var excludesIsPopulated = (query.Excludes is not null && query.Excludes.Count > 0);
            var modelProperties = model.GetType().GetProperties();

            if (includesIsPopulated && excludesIsPopulated)
                return new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Includes)} and {nameof(Query.Excludes)} cannot both be populated.");

            if (includesIsPopulated && !modelProperties.HasAllPropertyNames(query.Includes!, Delimiter.MongoDbChildProperty))
                return new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Includes)} cannot contain properties that the model does not have.");

            if (excludesIsPopulated && !modelProperties.HasAllPropertyNames(query.Excludes!, Delimiter.MongoDbChildProperty))
                return new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Excludes)} cannot contain properties that the model does not have.");

            if (query.Where is not null)
            {
                var conditionValidationResult = ValidateCondition(modelProperties, query.Where);
                if (!conditionValidationResult.IsValid)
                    return conditionValidationResult;
            }

            if (query.OrderBy is not null)
            {
                var orderByValidationResult = ValidateSorts(modelProperties, query.OrderBy);
                if (!orderByValidationResult.IsValid)
                    return orderByValidationResult;
            }

            if (query.Limit < 0)
                return new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.");

            if (query.Skip < 0)
                return new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Skip)} cannot be less than zero.");

            return new ValidationResult(true);
        }

        public ValidationResult ValidateCondition(PropertyInfo[] modelProperties, Condition condition)
        {
            if (condition.Field is null && condition.GroupedConditions is null)
                return new ValidationResult(false, $"A {nameof(Condition)} must contain either a {nameof(Condition.Field)} or {nameof(Condition.GroupedConditions)}.");

            if (condition.Field is not null)
            {
                var propertyInfo = modelProperties.GetProperty(condition.Field, Delimiter.MongoDbChildProperty);
                if (propertyInfo is null)
                    return new ValidationResult(false, $"A {nameof(Condition)} {nameof(Condition.Field)} contains a property {condition.Field} that the model does not have.");

                if (condition.ComparisonOperator is null)
                    return new ValidationResult(false, $"A {nameof(Condition)} cannot have a populated {nameof(Condition.Field)} and a null {nameof(Condition.ComparisonOperator)}.");

                if (!Operator.ComparisonAliasLookup.ContainsKey(condition.ComparisonOperator))
                    return new ValidationResult(false, $"{nameof(Condition.ComparisonOperator)} '{condition.ComparisonOperator}' must be found in {Operator.ComparisonAliasLookup}.");

                var comparisonOperator = Operator.ComparisonAliasLookup[condition.ComparisonOperator];

                var validationResult = ValidateQueryApplicationOptions(comparisonOperator);
                if (!validationResult.IsValid)
                    return validationResult;

                validationResult = ValidatePropertyQueryAttributes(propertyInfo, comparisonOperator);
                if (!validationResult.IsValid)
                    return validationResult;

                if (condition.Value is not null)
                {
                    if ((comparisonOperator == Operator.Contains
                    || comparisonOperator == Operator.StartsWith
                    || comparisonOperator == Operator.EndsWith)
                    && (condition.Value.Length == 0
                    || !condition.Value.All(Char.IsLetterOrDigit)))
                        return new ValidationResult(false, $"{nameof(Condition.ComparisonOperator)} '{condition.ComparisonOperator}' can only contain letters and numbers.");
                }
            }

            if (condition.GroupedConditions is not null)
            {
                foreach (var groupedCondition in condition.GroupedConditions)
                {
                    var validationResult = ValidateGroupedCondition(modelProperties, groupedCondition);
                    if (!validationResult.IsValid)
                        return validationResult;
                }
            }

            return new ValidationResult(true);
        }

        public ValidationResult ValidateQueryApplicationOptions(String? comparisonOperator)
        {
            if (comparisonOperator is not null)
            {
                if ((comparisonOperator == Operator.Contains && _applicationOptions.PreventAllQueryContains)
                || (comparisonOperator == Operator.StartsWith && _applicationOptions.PreventAllQueryStartsWith)
                || (comparisonOperator == Operator.EndsWith && _applicationOptions.PreventAllQueryEndsWith))
                    return new ValidationResult(false, $"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' may not be used.");
            }

            return new ValidationResult(true);
        }

        public ValidationResult ValidatePropertyQueryAttributes(PropertyInfo propertyInfo, String? comparisonOperator)
        {
            if (comparisonOperator is not null)
            {
                var attribute = propertyInfo.GetCustomAttribute<PreventQueryAttribute>();
                if (attribute is not null)
                {
                    if (!attribute.AllowsOperator(comparisonOperator))
                        return new ValidationResult(false, $"{nameof(Condition.ComparisonOperator)} '{comparisonOperator}' may not be used on the {propertyInfo.Name} property.");
                }
            }

            return new ValidationResult(true);
        }

        public ValidationResult ValidateGroupedCondition(PropertyInfo[] modelProperties, GroupedCondition groupedCondition)
        {
            if (groupedCondition.LogicalOperator is not null && !Operator.LogicalAliasLookup.ContainsKey(groupedCondition.LogicalOperator))
                return new ValidationResult(false, $"{nameof(GroupedCondition.LogicalOperator)} '{groupedCondition.LogicalOperator}' must be found in {Operator.LogicalAliasLookup}.");

            if (groupedCondition.Conditions is null || groupedCondition.Conditions.Count == 0)
                return new ValidationResult(false, $"{nameof(GroupedCondition.Conditions)} cannot be empty.");

            foreach (var condition in groupedCondition.Conditions)
            {
                var validationResult = ValidateCondition(modelProperties, condition);
                if (!validationResult.IsValid)
                    return validationResult;
            }

            return new ValidationResult(true);
        }

        public ValidationResult ValidateSorts(PropertyInfo[] modelProperties, IReadOnlyCollection<Sort> sorts)
        {
            foreach (var sort in sorts)
            {
                if (sort.Field is null)
                    return new ValidationResult(false, $"{nameof(Query.OrderBy)} cannot contain a {nameof(Sort)} with a null {nameof(Sort.Field)}.");

                if (!modelProperties.HasPropertyName(sort.Field, Delimiter.MongoDbChildProperty))
                    return new ValidationResult(false, $"A {nameof(Sort)} {nameof(Sort.Field)} contains a property {sort.Field} that the model does not have.");
            }

            return new ValidationResult(true);
        }

        private Boolean WillBeUpdated(String propertyName, IEnumerable<String>? propertiesToBeUpdated)
        {
            return propertiesToBeUpdated?.Contains(propertyName, StringComparer.OrdinalIgnoreCase) ?? false;
        }
    }
}
