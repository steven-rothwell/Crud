using Crud.Api.Attributes;
using Crud.Api.Enums;

namespace Crud.Api.Tests.Extensions
{
    public class PreventCrudAttributeTests
    {
        [Fact]
        public void AllowsCrudOperation_PreventedCrudOperationsCountIsZero_ReturnsFalse()
        {
            var preventCrudAttribute = new PreventCrudAttribute();
            var crudOperation = CrudOperation.Create;

            var result = preventCrudAttribute.AllowsCrudOperation(crudOperation);

            Assert.False(result);
        }

        [Fact]
        public void AllowsCrudOperation_PreventedCrudOperationsContainsCrudOperation_ReturnsFalse()
        {
            var preventCrudAttribute = new PreventCrudAttribute(CrudOperation.Create);
            var crudOperation = CrudOperation.Create;

            var result = preventCrudAttribute.AllowsCrudOperation(crudOperation);

            Assert.False(result);
        }

        [Fact]
        public void AllowsCrudOperation_EncompassingCrudOperationLookupContainsKeyCrudOperation_ReturnsPreventedCrudOperationsContainsEncompassingCrudOperationResult()
        {
            var preventCrudAttribute = new PreventCrudAttribute(CrudOperation.Read);
            var crudOperation = CrudOperation.ReadWithId;

            var result = preventCrudAttribute.AllowsCrudOperation(crudOperation);

            Assert.False(result);
        }

        [Fact]
        public void AllowsCrudOperation_EncompassingCrudOperationLookupDoesNotContainKeyCrudOperation_ReturnsTrue()
        {
            var preventCrudAttribute = new PreventCrudAttribute(CrudOperation.Read);
            var crudOperation = CrudOperation.Create;

            var result = preventCrudAttribute.AllowsCrudOperation(crudOperation);

            Assert.True(result);
        }

        [Theory]
        [ClassData(typeof(EncompassedReadOperations))]
        public void EncompassingCrudOperationLookup_ReadPrevented_AllEncompassedReadOperationsReturnFalse(CrudOperation crudOperation)
        {
            var preventCrudAttribute = new PreventCrudAttribute(CrudOperation.Read);

            var result = preventCrudAttribute.AllowsCrudOperation(crudOperation);

            Assert.False(result);
        }

        [Theory]
        [ClassData(typeof(EncompassedPartialUpdateOperations))]
        public void EncompassingCrudOperationLookup_PartialUpdatePrevented_AllEncompassedPartialUpdateOperationsReturnFalse(CrudOperation crudOperation)
        {
            var preventCrudAttribute = new PreventCrudAttribute(CrudOperation.PartialUpdate);

            var result = preventCrudAttribute.AllowsCrudOperation(crudOperation);

            Assert.False(result);
        }

        [Theory]
        [ClassData(typeof(EncompassedDeleteOperations))]
        public void EncompassingCrudOperationLookup_DeletePrevented_AllEncompassedDeleteOperationsReturnFalse(CrudOperation crudOperation)
        {
            var preventCrudAttribute = new PreventCrudAttribute(CrudOperation.Delete);

            var result = preventCrudAttribute.AllowsCrudOperation(crudOperation);

            Assert.False(result);
        }

        private class EncompassedReadOperations : TheoryData<CrudOperation>
        {
            public EncompassedReadOperations()
            {
                Add(CrudOperation.ReadWithId);
                Add(CrudOperation.ReadWithQueryParams);
                Add(CrudOperation.ReadWithQuery);
                Add(CrudOperation.ReadCount);
            }
        }

        private class EncompassedPartialUpdateOperations : TheoryData<CrudOperation>
        {
            public EncompassedPartialUpdateOperations()
            {
                Add(CrudOperation.PartialUpdateWithId);
                Add(CrudOperation.PartialUpdateWithQueryParams);
            }
        }

        private class EncompassedDeleteOperations : TheoryData<CrudOperation>
        {
            public EncompassedDeleteOperations()
            {
                Add(CrudOperation.DeleteWithId);
                Add(CrudOperation.DeleteWithQueryParams);
                Add(CrudOperation.DeleteWithQuery);
            }
        }
    }
}