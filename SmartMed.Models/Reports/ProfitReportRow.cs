using System;

namespace SmartMed.Models.Reports
{
    public class ProfitReportRow
    {
        public int SaleId { get; set; }
        public string SaleNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalCostOfGoodsSold { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
    }
}
