using Crud.Api.QueryModels;

namespace Crud.Api.Constants
{
    public static class ErrorMessage
    {
        public const String NotFoundRead = "No matching {0} found.";
        public const String NotFoundUpdate = "No matching {0} found to update.";
        public const String NotFoundDelete = "No matching {0} found to delete.";

        public const String BadRequestModelType = "No model type found.";
        public const String BadRequestBody = "Request body cannot be null or whitespace.";
        public const String BadRequestQuery = $"A {nameof(Query)} object cannot be created from the request body.";
    }
}
