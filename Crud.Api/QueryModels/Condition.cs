namespace Crud.Api.QueryModels
{
    /// <summary>
    /// Constrains what documents/rows are filtered on in the data store.
    /// </summary>
    public class Condition
    {
        /// <summary>
        /// Name of the field/column side being evaluated.
        /// Should be null if <see cref="GroupedConditions"/> is populated.
        /// </summary>
        public String? Field { get; set; }
        /// <summary>
        /// The operator used in the evaluation.
        /// Should be null if <see cref="GroupedConditions"/>is populated.
        /// </summary>
        public String? ComparisonOperator { get; set; }
        /// <summary>
        /// Value that the <see cref="ComparisonOperator"/> will compare the <see cref="Field"/> against in the evaluation.
        /// Should be null if <see cref="Values"/> or <see cref="GroupedConditions"/> is populated.
        /// </summary>
        public String? Value { get; set; }
        /// <summary>
        /// Values that the <see cref="ComparisonOperator"/> will compare the <see cref="Field"/> against in the evaluation.
        /// Should be null if <see cref="Value"/> or <see cref="GroupedConditions"/> is populated.
        /// </summary>
        /// <value></value>
        public IReadOnlyCollection<String>? Values { get; set; }
        /// <summary>
        /// Groups of conditions used for complex logic to constrain what documents/rows are filtered on in the data store.
        /// </summary>
        public IReadOnlyCollection<GroupedCondition>? GroupedConditions { get; set; }
    }
}
