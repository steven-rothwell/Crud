using Crud.Api.Validators;
using DataAnnotations = System.ComponentModel.DataAnnotations;

namespace Crud.Api
{
    public static class ObjectExtensions
    {
        public static ValidationResult ValidateDataAnnotations(this Object model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            var results = new List<DataAnnotations.ValidationResult>();
            var validationContext = new DataAnnotations.ValidationContext(model);
            if (!DataAnnotations.Validator.TryValidateObject(model, validationContext, results))
                return new ValidationResult(false, results.First().ErrorMessage);

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateDataAnnotations(this Object model, IReadOnlyCollection<String> propertiesToValidate)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            if (propertiesToValidate is null)
                throw new ArgumentNullException(nameof(propertiesToValidate));

            var results = new List<DataAnnotations.ValidationResult>();
            var validationContext = new DataAnnotations.ValidationContext(model);
            if (!DataAnnotations.Validator.TryValidateObject(model, validationContext, results))
            {
                var firstResult = results.Where(result => result.MemberNames.Any(memberName => propertiesToValidate.Contains(memberName))).FirstOrDefault();
                if (firstResult is not null)
                    return new ValidationResult(false, firstResult.ErrorMessage);
            }

            return new ValidationResult(true);
        }
    }
}
