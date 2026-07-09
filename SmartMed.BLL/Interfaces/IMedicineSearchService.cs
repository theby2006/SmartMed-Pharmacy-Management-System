using System.Collections.Generic;
using SmartMed.BLL.Models;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    /// <summary>
    /// In-memory search/filter/sort algorithms over a medicine catalogue,
    /// implemented directly (not delegated to SQL) so their complexity is
    /// demonstrable and testable in isolation.
    /// </summary>
    public interface IMedicineSearchService
    {
        List<Medicine> LinearSearchByName(List<Medicine> medicines, string term);

        List<Medicine> FilterByCategory(List<Medicine> medicines, int categoryId);

        List<Medicine> FilterByPriceRange(List<Medicine> medicines, decimal minPrice, decimal maxPrice);

        List<Medicine> SortByPrice(List<Medicine> medicines);

        List<Medicine> BinarySearchByPrice(List<Medicine> priceSortedMedicines, decimal targetPrice);

        OperationResult<List<Medicine>> Search(MedicineSearchCriteria criteria);
    }
}
