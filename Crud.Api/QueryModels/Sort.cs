namespace Crud.Api.QueryModels
{
    /// <summary>
    /// Describes what and how something will be sorted.
    /// </summary>
    public class Sort
    {
        /// <summary>
        /// Name of the field/column being sorted.
        /// </summary>
        public String? Field { get; set; }
        /// <summary>
        /// If the <see cref="Field"/></param> will be in descending order.
        /// </summary>
        public Boolean? IsDescending { get; set; }
    }
}
