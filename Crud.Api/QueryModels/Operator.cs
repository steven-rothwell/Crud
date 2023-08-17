namespace Crud.Api.QueryModels
{
    public static class Operator
    {
        public const String And = "&&";
        public const String Or = "||";
        public const String Equality = "==";
        public const String Inequality = "!=";
        public const String GreaterThan = ">";
        public const String GreaterThanOrEquals = ">=";
        public const String LessThan = "<";
        public const String LessThanOrEquals = "<=";
        public const String In = "IN";
        public const String NotIn = "NIN";
        public const String Contains = "CONTAINS";
        public const String StartsWith = "STARTSWITH";
        public const String EndsWith = "ENDSWITH";

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
            { GreaterThanOrEquals, GreaterThanOrEquals },
            { "GreaterThanOrEquals", GreaterThanOrEquals },
            { "GTE", GreaterThanOrEquals },
            { LessThan, LessThan },
            { "LessThan", LessThan },
            { "LT", LessThan },
            { LessThanOrEquals, LessThanOrEquals },
            { "LessThanOrEquals", LessThanOrEquals },
            { "LTE", LessThanOrEquals },
            { In, In },
            { NotIn, NotIn },
            { "NotIn", NotIn },
            { Contains, Contains },
            { StartsWith, StartsWith },
            { EndsWith, EndsWith }
        };
    }
}
