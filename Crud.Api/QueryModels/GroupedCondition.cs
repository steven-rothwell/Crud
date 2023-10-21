namespace Crud.Api.QueryModels
{
    /// <summary>
    /// Groups of conditions used for complex logic to constrain what documents/rows are filtered on in the data store.
    /// </summary>
    public class GroupedCondition
    {
        /// <summary>
        /// The operator applied between each condition in <see cref="Conditions"/></param>.
        /// </summary>
        public String? LogicalOperator { get; set; }
        /// <summary>
        /// All conditions have the same <see cref="LogicalOperator"/></param> applied between each condition.
        /// </summary>
        public IReadOnlyCollection<Condition>? Conditions { get; set; }
    }
}
