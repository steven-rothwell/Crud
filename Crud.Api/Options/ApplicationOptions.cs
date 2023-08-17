namespace Crud.Api.Options
{
    public class ApplicationOptions
    {
        public Boolean ShowExceptions { get; set; }
        public Boolean ValidateQuery { get; set; }
        public Boolean PreventAllQueryContains { get; set; }
        public Boolean PreventAllQueryStartsWith { get; set; }
        public Boolean PreventAllQueryEndsWith { get; set; }
    }
}
