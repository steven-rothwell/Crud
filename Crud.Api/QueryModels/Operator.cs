namespace Crud.Api.QueryModels
{
    public static class Operator
    {
        public const String And = "&&";
        public const String Or = "||";
        public const String Equality = "==";
        public const String Inequality = "!=";
        public const String GreaterThan = ">";
        public const String GreaterThanOrEqual = ">=";
        public const String LessThan = "<";
        public const String LessThanOrEqual = "<=";
        public const String In = "IN";
        public const String NotIn = "NIN";

        public static Dictionary<String, String> LogicalAliasLookup = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase)
        {
            { And, And },
            { "AND", And },
            { Or, Or },
            { "OR", Or }
        };

        public static Dictionary<String, String> ComparisonAliasLookup = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase)
        {
            { Equality, Equality },
            { "Equals", Equality },
            { "EQ", Equality },
            { Inequality, Inequality },
            { "NotEquals", Inequality },
            { "NE", Inequality },
            { GreaterThan, GreaterThan },
            { "GreaterThan", GreaterThan },
            { "GT", GreaterThan },
            { GreaterThanOrEqual, GreaterThanOrEqual },
            { "GreaterThanOrEquals", GreaterThanOrEqual },
            { "GTE", GreaterThanOrEqual },
            { LessThan, LessThan },
            { "LessThan", LessThan },
            { "LT", LessThan },
            { LessThanOrEqual, LessThanOrEqual },
            { "LessThanOrEqual", LessThanOrEqual },
            { "LTE", LessThanOrEqual },
            { In, In },
            { NotIn, NotIn },
            { "NotIn", NotIn }
        };
    }
}
