namespace Crud.Api.QueryModels
{
    /// <summary>
    /// Constrains what documents/rows are returned from the database.
    /// </summary>
    public class Condition
    {
        /// <summary>
        /// Name of the left side being evaluated.
        /// /// Should be null if only <see cref="ComparisonOperator"/></param> is populated.
        /// Should be null if <see cref="GroupedConditions"/></param> is populated.
        /// </summary>
        public String? Field { get; set; }
        /// <summary>
        /// The operator used in the evaluation.
        /// Should be null if <see cref="GroupedConditions"/></param> is populated.
        /// </summary>
        /// <value></value>
        public String? ComparisonOperator { get; set; }
        /// <summary>
        /// Value that the <see cref="ComparisonOperator"/></param> will compare the <see cref="Field"/></param> against in the evaluation.
        /// </summary>
        /// <value></value>
        public String? Value { get; set; }
        public IReadOnlyCollection<GroupedCondition>? GroupedConditions { get; set; }
    }
}
