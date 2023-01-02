using Crud.Api.Validators;
using DataAnnotations = System.ComponentModel.DataAnnotations;

namespace Crud.Api
{
    public static class ObjectExtensions
    {
        public static ValidationResult ValidateDataAnnotations(this Object model, Boolean validateChildModels = true, IReadOnlyCollection<String>? propertiesToValidate = null, Char childPropertyNameDelimiter = default)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            var results = new List<DataAnnotations.ValidationResult>();
            var validationContext = new DataAnnotations.ValidationContext(model);
            if (!DataAnnotations.Validator.TryValidateObject(model, validationContext, results))
            {
                if (propertiesToValidate is null)
                {
                    return new ValidationResult(false, results.First().ErrorMessage);
                }
                else
                {
                    var firstResult = results.Where(result => result.MemberNames.Any(memberName => propertiesToValidate.Contains(memberName))).FirstOrDefault();
                    if (firstResult is not null)
                        return new ValidationResult(false, firstResult.ErrorMessage);
                }
            }

            if (validateChildModels)
            {
                var classPropertyValues = model.GetClassPropertyValues();
                if (classPropertyValues is not null)
                {
                    foreach (var classPropertyValue in classPropertyValues)
                    {
                        if (classPropertyValue is null)
                            continue;

                        var validationResult = classPropertyValue.ValidateDataAnnotations(validateChildModels, propertiesToValidate?.Select(propertyToValidate => propertyToValidate.GetValueAfterFirstDelimiter(childPropertyNameDelimiter)).ToList());
                        if (!validationResult.IsValid)
                            return validationResult;
                    }
                }
            }

            return new ValidationResult(true);
        }

        public static IEnumerable<Object?>? GetClassPropertyValues(this Object model)
        {
            return model.GetType().GetProperties().Where(property => property.PropertyType.IsClass && property.PropertyType != typeof(string))?.Select(property => property.GetValue(model, null));
        }
    }
}
