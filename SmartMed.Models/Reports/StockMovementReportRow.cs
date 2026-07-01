using System;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Reports
{
    public class StockMovementReportRow
    {
        public DateTime MovementDate { get; set; }
        public string MedicineName { get; set; }
        public string BatchNumber { get; set; }
        public MovementType MovementType { get; set; }
        public int Quantity { get; set; }
        public string ReferenceType { get; set; }
        public int ReferenceId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
