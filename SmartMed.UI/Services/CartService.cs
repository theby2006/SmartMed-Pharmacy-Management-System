using System;
using System.Collections.Generic;
using System.Linq;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.UI.Services
{
    public class CartLine
    {
        public Medicine Medicine { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => Medicine.UnitPrice * Quantity * (1 - Medicine.DiscountPercent / 100m);
        public decimal LineDiscount => Medicine.UnitPrice * Quantity * (Medicine.DiscountPercent / 100m);
    }

    /// <summary>
    /// Ephemeral, in-memory shopping cart scoped to one customer shell
    /// session. This is UI-layer state, not persisted business data, so it
    /// intentionally does not live in the BLL alongside the real domain
    /// services it is used with (<see cref="IOrderService"/>,
    /// <see cref="IPricingService"/>).
    /// </summary>
    public class CartService
    {
        private readonly List<CartLine> _lines = new List<CartLine>();
        private readonly IPricingService _pricingService;

        public event EventHandler CartChanged;

        public CartService(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        public IReadOnlyList<CartLine> Lines => _lines;

        public int ItemCount => _lines.Sum(l => l.Quantity);

        public void AddItem(Medicine medicine, int quantity)
        {
            CartLine existing = _lines.FirstOrDefault(l => l.Medicine.Id == medicine.Id);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _lines.Add(new CartLine { Medicine = medicine, Quantity = quantity });
            }

            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateQuantity(int medicineId, int quantity)
        {
            CartLine line = _lines.FirstOrDefault(l => l.Medicine.Id == medicineId);
            if (line == null) return;

            if (quantity <= 0)
            {
                _lines.Remove(line);
            }
            else
            {
                line.Quantity = quantity;
            }

            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveItem(int medicineId)
        {
            _lines.RemoveAll(l => l.Medicine.Id == medicineId);
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            _lines.Clear();
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool RequiresPrescription => _lines.Any(l => l.Medicine.RequiresPrescription);

        public decimal SubTotal => _lines.Sum(l => _pricingService.CalculateSubTotal(l.Quantity, l.Medicine.UnitPrice));

        public decimal DiscountAmount => _lines.Sum(l =>
            _pricingService.CalculateDiscountAmount(
                _pricingService.CalculateSubTotal(l.Quantity, l.Medicine.UnitPrice),
                l.Medicine.DiscountPercent));

        public decimal GrandTotal => _pricingService.CalculateGrandTotal(SubTotal, DiscountAmount, 0m);
    }
}
