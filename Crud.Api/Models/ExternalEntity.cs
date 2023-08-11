using System.Text.Json.Serialization;

namespace Crud.Api.Models
{
    public abstract class ExternalEntity : IExternalEntity
    {
        [JsonPropertyOrder(order: -1)]  // Properties without this attribute default to order 0. Must be negative to come before them.
        [JsonPropertyName("id")]
        public Guid? ExternalId { get; set; }
    }
}
