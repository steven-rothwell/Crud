namespace Crud.Api.Services
{
    public class QueryCollectionService : IQueryCollectionService
    {
        public QueryCollectionService() { }

        public Dictionary<String, String> ConvertToDictionary(IQueryCollection queryCollection)
        {
            return queryCollection.ToDictionary(query => query.Key, query => query.Value.ToString());
        }
    }
}
