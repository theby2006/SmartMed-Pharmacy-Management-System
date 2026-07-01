using System;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Reports
{
    public class ReportCriteria
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? CashierId { get; set; }
        public int? SupplierId { get; set; }
        public int? CategoryId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public ReportPeriod Period { get; set; } = ReportPeriod.ThisMonth;
        public string Keyword { get; set; }
        public int? MedicineId { get; set; }
    }
}
