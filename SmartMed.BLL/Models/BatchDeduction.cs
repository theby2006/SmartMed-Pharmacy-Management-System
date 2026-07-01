using System;

namespace SmartMed.BLL.Models
{
    public class BatchDeduction
    {
        public int MedicineId { get; set; }
        public int StockBatchId { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int QuantityDeducted { get; set; }
        public decimal SellingPrice { get; set; }
    }
}
