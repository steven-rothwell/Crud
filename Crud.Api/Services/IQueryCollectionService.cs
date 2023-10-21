namespace Crud.Api.Services
{
    public interface IQueryCollectionService
    {
        Dictionary<String, String> ConvertToDictionary(IQueryCollection queryCollection);
    }
}
