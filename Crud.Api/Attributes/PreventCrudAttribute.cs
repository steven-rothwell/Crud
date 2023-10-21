using Crud.Api.Enums;

namespace Crud.Api.Attributes
{
    /// <summary>
    /// Decorate a class to prevent CRUD operations. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PreventCrudAttribute : Attribute
    {
        private HashSet<CrudOperation> _preventedCrudOperations;

        private static IReadOnlyDictionary<CrudOperation, CrudOperation> _encompassingCrudOperationLookup = new Dictionary<CrudOperation, CrudOperation>
        {
            { CrudOperation.ReadWithId, CrudOperation.Read },
            { CrudOperation.ReadWithQueryParams, CrudOperation.Read },
            { CrudOperation.ReadWithQuery, CrudOperation.Read },
            { CrudOperation.ReadCount, CrudOperation.Read },
            { CrudOperation.PartialUpdateWithId, CrudOperation.PartialUpdate },
            { CrudOperation.PartialUpdateWithQueryParams, CrudOperation.PartialUpdate },
            { CrudOperation.DeleteWithId, CrudOperation.Delete },
            { CrudOperation.DeleteWithQueryParams, CrudOperation.Delete },
            { CrudOperation.DeleteWithQuery, CrudOperation.Delete }
        };

        /// <summary>
        /// Specific CRUD operations to prevent may be specified. If no operations are specified, all CRUD operations are prevented. 
        /// </summary>
        /// <param name="crudOperations">CRUD operations to be prevented.</param>
        public PreventCrudAttribute(params CrudOperation[] crudOperations)
        {
            _preventedCrudOperations = new HashSet<CrudOperation>(crudOperations);
        }

        public Boolean AllowsCrudOperation(CrudOperation crudOperation)
        {
            if (_preventedCrudOperations.Count == 0)
                return false;

            if (_preventedCrudOperations.Contains(crudOperation))
                return false;

            if (_encompassingCrudOperationLookup.ContainsKey(crudOperation))
            {
                var encompassingCrudOperation = _encompassingCrudOperationLookup[crudOperation];

                return !_preventedCrudOperations.Contains(encompassingCrudOperation);
            }

            return true;
        }
    }
}
