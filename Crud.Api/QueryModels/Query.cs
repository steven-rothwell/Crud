namespace Crud.Api.QueryModels
{
    /// <summary>
    /// Determines what is returned from the database.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Fields/columns that will be returned from the database.
        /// If this and <see cref="Excludes"/></param> are null, all fields/columns are returned.
        /// </summary>
        public HashSet<String>? Includes { get; set; }
        /// <summary>
        /// Fields/columns that will not be returned from the database.
        /// If this and <see cref="Includes"/></param> are null, all fields/columns are returned.
        /// </summary>
        public HashSet<String>? Excludes { get; set; }
        /// <summary>
        /// Documents/rows that will be returned from the database.
        /// </summary>
        public Condition? Where { get; set; }
        /// <summary>
        /// In what order the documents/rows will be returned from the database.
        /// </summary>
        public IReadOnlyCollection<Sort>? OrderBy { get; set; }
        /// <summary>
        /// Sets the max number of documents/rows that will be returned from the database.
        /// </summary>
        public Int32? Limit { get; set; }
        /// <summary>
        /// Sets how many documents/rows to skip over.
        /// </summary>
        public Int32? Skip { get; set; }
    }
}
