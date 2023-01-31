namespace Crud.Api.QueryModels
{
    public static class Operator
    {
        public const String And = "&&";
        public const String Or = "||";
        public const String Equality = "==";

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
            { "EQUALS", Equality },
        };
    }
}
