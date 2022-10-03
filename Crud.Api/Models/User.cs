using System.ComponentModel.DataAnnotations.Schema;

namespace Crud.Api.Models
{
    [Table("users")]
    public class User : IExternalEntity
    {
        public Guid? ExternalId { get; set; }
        public String? Name { get; set; }
        public Address? Address { get; set; }
        public Int32? Age { get; set; }
        public String? HairColor { get; set; }
        public ICollection<String>? FavoriteThings { get; set; }
    }
}
