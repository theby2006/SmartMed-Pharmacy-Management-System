using System.Collections.Generic;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Models;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    /// <summary>
    /// Implements medicine catalogue search entirely in memory over a
    /// <see cref="List{Medicine}"/> snapshot, rather than delegating to a SQL
    /// WHERE clause, so the search/filter/sort algorithms below are the
    /// system's own code and can be reasoned about (and tested) independently
    /// of the database.
    /// </summary>
    public class MedicineSearchService : IMedicineSearchService
    {
        private readonly IMedicineRepository _medicineRepository;

        public MedicineSearchService(IMedicineRepository medicineRepository)
        {
            Guard.AgainstNull(medicineRepository, nameof(medicineRepository));
            _medicineRepository = medicineRepository;
        }

        /// <summary>
        /// Case-insensitive substring match against Name and Brand.
        /// Time complexity: O(n), where n is the number of medicines supplied -
        /// every element is visited exactly once.
        /// </summary>
        public List<Medicine> LinearSearchByName(List<Medicine> medicines, string term)
        {
            List<Medicine> results = new List<Medicine>();

            if (medicines == null || string.IsNullOrWhiteSpace(term))
                return results;

            string needle = term.Trim().ToLowerInvariant();

            for (int i = 0; i < medicines.Count; i++)
            {
                Medicine medicine = medicines[i];
                bool nameMatches = medicine.Name != null && medicine.Name.ToLowerInvariant().Contains(needle);
                bool brandMatches = medicine.Brand != null && medicine.Brand.ToLowerInvariant().Contains(needle);

                if (nameMatches || brandMatches)
                    results.Add(medicine);
            }

            return results;
        }

        /// <summary>
        /// Predicate filter over CategoryId.
        /// Time complexity: O(n) - a single pass over the supplied list.
        /// </summary>
        public List<Medicine> FilterByCategory(List<Medicine> medicines, int categoryId)
        {
            List<Medicine> results = new List<Medicine>();

            if (medicines == null)
                return results;

            for (int i = 0; i < medicines.Count; i++)
            {
                if (medicines[i].CategoryId == categoryId)
                    results.Add(medicines[i]);
            }

            return results;
        }

        /// <summary>
        /// Predicate filter over UnitPrice, inclusive of both bounds.
        /// Time complexity: O(n) - a single pass over the supplied list.
        /// </summary>
        public List<Medicine> FilterByPriceRange(List<Medicine> medicines, decimal minPrice, decimal maxPrice)
        {
            List<Medicine> results = new List<Medicine>();

            if (medicines == null)
                return results;

            for (int i = 0; i < medicines.Count; i++)
            {
                decimal price = medicines[i].UnitPrice;
                if (price >= minPrice && price <= maxPrice)
                    results.Add(medicines[i]);
            }

            return results;
        }

        /// <summary>
        /// Insertion sort by ascending UnitPrice. The catalogue is small
        /// enough (tens to low hundreds of rows) that an O(n^2) worst case is
        /// not a practical concern, and insertion sort is stable and easy to
        /// verify by inspection - both useful properties for a method whose
        /// output directly feeds <see cref="BinarySearchByPrice"/>.
        /// Time complexity: O(n^2) worst case, O(n) best case (already sorted).
        /// </summary>
        public List<Medicine> SortByPrice(List<Medicine> medicines)
        {
            List<Medicine> sorted = medicines == null ? new List<Medicine>() : new List<Medicine>(medicines);

            for (int i = 1; i < sorted.Count; i++)
            {
                Medicine current = sorted[i];
                int j = i - 1;

                while (j >= 0 && sorted[j].UnitPrice > current.UnitPrice)
                {
                    sorted[j + 1] = sorted[j];
                    j--;
                }

                sorted[j + 1] = current;
            }

            return sorted;
        }

        /// <summary>
        /// Classic binary search for an exact UnitPrice match. The input list
        /// MUST already be sorted ascending by UnitPrice (see
        /// <see cref="SortByPrice"/>) - this method does not sort its input.
        /// Because multiple medicines can share the same price, once an
        /// initial match is found the method scans both directions from that
        /// index to collect every medicine at that price, which stays within
        /// O(log n + k) where k is the number of matches.
        /// Time complexity: O(log n) to locate a match, O(k) to collect
        /// duplicates, so O(log n + k) overall.
        /// </summary>
        public List<Medicine> BinarySearchByPrice(List<Medicine> priceSortedMedicines, decimal targetPrice)
        {
            List<Medicine> results = new List<Medicine>();

            if (priceSortedMedicines == null || priceSortedMedicines.Count == 0)
                return results;

            int low = 0;
            int high = priceSortedMedicines.Count - 1;
            int matchIndex = -1;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                decimal midPrice = priceSortedMedicines[mid].UnitPrice;

                if (midPrice == targetPrice)
                {
                    matchIndex = mid;
                    break;
                }

                if (midPrice < targetPrice)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            if (matchIndex == -1)
                return results;

            int left = matchIndex;
            while (left >= 0 && priceSortedMedicines[left].UnitPrice == targetPrice)
            {
                results.Add(priceSortedMedicines[left]);
                left--;
            }

            int right = matchIndex + 1;
            while (right < priceSortedMedicines.Count && priceSortedMedicines[right].UnitPrice == targetPrice)
            {
                results.Add(priceSortedMedicines[right]);
                right++;
            }

            return results;
        }

        /// <summary>
        /// Orchestrates the algorithms above against the current medicine
        /// catalogue: a name match (linear scan) narrows the set first, then
        /// category and price-range filters are applied in sequence. When
        /// the caller supplies only an exact price (no name/category/range),
        /// the search takes the O(log n) binary-search path instead of a
        /// linear scan, since that is the one case where the data can be
        /// sorted once and probed directly.
        /// </summary>
        public OperationResult<List<Medicine>> Search(MedicineSearchCriteria criteria)
        {
            Guard.AgainstNull(criteria, nameof(criteria));

            List<Medicine> catalogue = _medicineRepository.GetAll();

            bool hasNameTerm = !string.IsNullOrWhiteSpace(criteria.NameTerm);
            bool hasCategory = criteria.CategoryId.HasValue;
            bool hasRange = criteria.MinPrice.HasValue || criteria.MaxPrice.HasValue;
            bool onlyExactPrice = criteria.ExactPrice.HasValue && !hasNameTerm && !hasCategory && !hasRange;

            if (onlyExactPrice)
            {
                List<Medicine> sorted = SortByPrice(catalogue);
                List<Medicine> exactMatches = BinarySearchByPrice(sorted, criteria.ExactPrice.Value);
                return OperationResult<List<Medicine>>.Success(exactMatches);
            }

            List<Medicine> working = catalogue;

            if (hasNameTerm)
                working = LinearSearchByName(working, criteria.NameTerm);

            if (hasCategory)
                working = FilterByCategory(working, criteria.CategoryId.Value);

            if (hasRange)
            {
                decimal min = criteria.MinPrice ?? 0m;
                decimal max = criteria.MaxPrice ?? decimal.MaxValue;
                working = FilterByPriceRange(working, min, max);
            }

            return OperationResult<List<Medicine>>.Success(working);
        }
    }
}
