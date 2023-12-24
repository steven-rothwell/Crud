using Crud.Api.Attributes;

namespace Crud.Api.Models
{

    [PreventCrud]
    /// <summary>
    /// This is used to test the Crud application and as an example model. This may be removed in any forked application.
    /// </summary>
    public class Address
    {
        public String? Street { get; set; }
        public String? City { get; set; }
        public String? State { get; set; }
    }
}
