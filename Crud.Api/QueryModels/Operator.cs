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
        /// <summary>
        /// If any value in <see cref="Condition.Field"/> matches any value in <see cref="Condition.Values"/>.
        /// </summary>
        public const String In = "IN";
        /// <summary>
        /// If all values in <see cref="Condition.Field"/> do not match any value in <see cref="Condition.Values"/>.
        /// </summary>
        public const String NotIn = "NIN";
        /// <summary>
        /// If all values in <see cref="Condition.Values"/> match any value in <see cref="Condition.Field"/>.
        /// </summary>
        public const String All = "ALL";
        /// <summary>
        /// For use with <see cref="Condition.Field"/> properties of type <see cref="String"/>. If value in <see cref="Condition.Field"/> contains the value in <see cref="Condition.Values"/>.
        /// </summary>
        public const String Contains = "CONTAINS";
        /// <summary>
        /// For use with <see cref="Condition.Field"/> properties of type <see cref="String"/>. If value in <see cref="Condition.Field"/> starts with the value in <see cref="Condition.Values"/>.
        /// </summary>
        public const String StartsWith = "STARTSWITH";
        /// <summary>
        /// For use with <see cref="Condition.Field"/> properties of type <see cref="String"/>. If value in <see cref="Condition.Field"/> ends with the value in <see cref="Condition.Values"/>.
        /// </summary>
        public const String EndsWith = "ENDSWITH";

        public static IReadOnlyDictionary<String, String> LogicalAliasLookup = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase)
        {
            { And, And },
            { "AND", And },
            { Or, Or },
            { "OR", Or }
        };

        public static IReadOnlyDictionary<String, String> ComparisonAliasLookup = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase)
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
            { All, All },
            { Contains, Contains },
            { StartsWith, StartsWith },
            { EndsWith, EndsWith }
        };
    }
}
