namespace SmartMed.BLL.Models
{
    /// <summary>
    /// Criteria bag consumed by <see cref="Interfaces.IMedicineSearchService.Search"/>.
    /// All fields are optional; omitted fields are simply not applied as filters.
    /// </summary>
    public class MedicineSearchCriteria
    {
        public string NameTerm { get; set; }

        public int? CategoryId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// When set, and no other filter is supplied, the search uses
        /// <see cref="Interfaces.IMedicineSearchService.BinarySearchByPrice"/>
        /// instead of a linear scan.
        /// </summary>
        public decimal? ExactPrice { get; set; }
    }
}
