namespace Crud.Api.Attributes
{
    /// <summary>
    /// Decorate a property to prevent Query operators. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PreventQueryAttribute : Attribute
    {
        private HashSet<String> _preventedOperators;

        /// <summary>
        /// Specific Query operators to prevent may be specified. If no operators are specified, all Query operators are prevented. Suggest using <see cref="QueryModels.Operator"/> constants.
        /// </summary>
        /// <param name="operators">Query operators to be prevented.</param>
        public PreventQueryAttribute(params String[] operators)
        {
            _preventedOperators = new HashSet<string>(operators);
        }

        public Boolean AllowsOperator(String @operator)
        {
            if (_preventedOperators.Count == 0)
                return false;

            if (_preventedOperators.Contains(@operator))
                return false;

            return true;
        }
    }
}
