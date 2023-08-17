namespace Crud.Api.Validators.Attributes
{
    /// <summary>
    /// Decorating a property with this attribute will prevent querying with the <see cref="QueryModels.Operator.Contains" /> operator. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PreventQueryEndsWithAttribute : Attribute
    {
        // No logic needed as this is handled in the validator.
    }
}
