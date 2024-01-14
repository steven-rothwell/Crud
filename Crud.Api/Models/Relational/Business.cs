namespace Crud.Api.Models
{
    /// <summary>
    /// This is used to test the Crud application and as an example model. This may be removed in any forked application.
    /// </summary>
    public class Business : RelationalEntity
    {
        public String? Name { get; set; }
        public String? Slogan { get; set; }
        public Owner? Owner { get; set; }
    }
}
