using Crud.Api.Attributes;

namespace Crud.Api.Models
{

    [PreventCrud]
    public class Address
    {
        public String? Street { get; set; }
        public String? City { get; set; }
        public String? State { get; set; }
    }
}
