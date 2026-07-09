using System;
using System.Collections.Generic;
using SmartMed.Models.Common;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; }

        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string PrescriptionFilePath { get; set; }

        public string Notes { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
