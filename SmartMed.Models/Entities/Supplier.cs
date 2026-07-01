using SmartMed.Models.Common;

namespace SmartMed.Models.Entities
{
    public class Supplier : BaseEntity
    {
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string TaxNumber { get; set; }
        public string Notes { get; set; }
    }
}
