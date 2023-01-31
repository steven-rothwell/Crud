namespace Crud.Api.QueryModels
{
    /// <summary>
    /// Determines what is returned from the database.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Fields/columns that will be returned from the database.
        /// </summary>
        public HashSet<String>? Includes { get; set; }
        /// <summary>
        /// Fields/columns that will not be returned from the database.
        /// </summary>
        public HashSet<String>? Excludes { get; set; }
        /// <summary>
        /// Documents/rows that will be returned from the database.
        /// </summary>
        public Condition? Where { get; set; }

        // Sort
        // Take
        // Skip
    }
}
