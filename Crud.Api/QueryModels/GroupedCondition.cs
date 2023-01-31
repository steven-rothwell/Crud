namespace Crud.Api.QueryModels
{
    public class GroupedCondition
    {
        public String? LogicalOperator { get; set; }
        public IReadOnlyCollection<Condition>? Conditions { get; set; }
    }
}
