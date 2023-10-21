namespace Crud.Api.Enums
{
    public enum CrudOperation
    {
        Create = 0,
        /// <summary>
        /// Encompasses all Read operations.
        /// </summary>
        Read,
        ReadWithId,
        ReadWithQueryParams,
        ReadWithQuery,
        ReadCount,
        Update,
        /// <summary>
        /// Encompasses all Partial Update operations.
        /// </summary>
        PartialUpdate,
        PartialUpdateWithId,
        PartialUpdateWithQueryParams,
        /// <summary>
        /// Encompasses all Delete operations.
        /// </summary>
        Delete,
        DeleteWithId,
        DeleteWithQueryParams,
        DeleteWithQuery
    }
}
