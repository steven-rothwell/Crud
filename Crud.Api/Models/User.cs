using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crud.Api.Attributes;
using Crud.Api.QueryModels;

namespace Crud.Api.Models
{
    [Table("users")]
    public class User : ExternalEntity
    {
        [Required]
        public String? Name { get; set; }
        public Address? Address { get; set; }
        [Range(0, Int32.MaxValue)]
        public Int32? Age { get; set; }
        [PreventQuery(Operator.Contains)]
        public String? HairColor { get; set; }
        public ICollection<String>? FavoriteThings { get; set; }
        public ICollection<Address>? FormerAddresses { get; set; }
    }
}
