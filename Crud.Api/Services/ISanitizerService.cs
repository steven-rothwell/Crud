namespace Crud.Api.Services
{
    public interface ISanitizerService
    {
        String SanitizeTypeName(String? className);
    }
}
