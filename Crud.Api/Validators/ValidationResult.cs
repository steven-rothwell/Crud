namespace Crud.Api.Validators
{
    public class ValidationResult
    {
        public ValidationResult() { }

        public ValidationResult(Boolean isValid)
        {
            IsValid = isValid;
        }

        public ValidationResult(Boolean isValid, String message)
        {
            IsValid = isValid;
            Message = message;
        }

        public Boolean IsValid { get; set; }
        public String? Message { get; set; }
    }
}
