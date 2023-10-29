using System.Text.RegularExpressions;
using Crud.Api.Constants;

namespace Crud.Api.Services
{
    public class SanitizerService : ISanitizerService
    {
        public String SanitizeTypeName(String? typeName)
        {
            if (String.IsNullOrWhiteSpace(typeName))
                return Default.TypeName;

            string sanitizedTypeName = Regex.Replace(typeName, @"[^@\w]", String.Empty);

            if (sanitizedTypeName.Length == 0)
                return Default.TypeName;

            return sanitizedTypeName;
        }
    }
}
