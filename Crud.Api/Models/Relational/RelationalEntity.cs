using System.Text.Json.Serialization;

namespace Crud.Api.Models
{
    public abstract class RelationalEntity : IExternalEntity
    {
        /// <summary>
        /// This is used as an identity clusted primary key and not exposed externally.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Int32 Id { get; set; }
        [JsonPropertyOrder(order: -1)]  // Properties without this attribute default to order 0. Must be negative to come before them.
        [JsonPropertyName("id")]
        public Guid? ExternalId { get; set; }
    }
}
