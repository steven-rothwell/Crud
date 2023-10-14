namespace Crud.Api.Attributes
{
    /// <summary>
    /// Decorate a property to prevent Query operators. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PreventQueryAttribute : Attribute
    {
        private String[] _preventedOperators;

        /// <summary>
        /// Specific Query operators to prevent may be specified. If no operators are specified, all Query operators are prevented. 
        /// </summary>
        /// <param name="operators">Query operators to be prevented.</param>
        public PreventQueryAttribute(params String[] operators)
        {
            _preventedOperators = operators;
        }
    }
}
