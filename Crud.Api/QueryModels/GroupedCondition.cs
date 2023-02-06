namespace Crud.Api.QueryModels
{
    /// <summary>
    /// Groups conditions together to constrains what documents/rows are returned from the database.
    /// </summary>
    public class GroupedCondition
    {
        /// <summary>
        /// The operator applied between each condition in <see cref="Conditions"/></param>.
        /// </summary>
        /// <value></value>
        public String? LogicalOperator { get; set; }
        /// <summary>
        /// All conditions have the same <see cref="LogicalOperator"/></param> applied between each condition.
        /// </summary>
        /// <value></value>
        public IReadOnlyCollection<Condition>? Conditions { get; set; }
    }
}
