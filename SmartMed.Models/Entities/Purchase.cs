using System;
using System.Collections.Generic;
using SmartMed.Models.Common;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Entities
{
    public class Purchase : BaseEntity
    {
        public string PurchaseNumber { get; set; }

        public DateTime PurchaseDate { get; set; }

        public int SupplierId { get; set; }

        public string InvoiceNumber { get; set; }

        public string Remarks { get; set; }

        public int CreatedByUserId { get; set; }

        public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

        public DateTime? ConfirmedDate { get; set; }

        public decimal TotalAmount { get; set; }

        public List<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }
}
