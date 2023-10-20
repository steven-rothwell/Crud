using System.Text.Json;

namespace Crud.Api.Constants
{
    public static class JsonSerializerOption
    {
        public static readonly JsonSerializerOptions Default = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
