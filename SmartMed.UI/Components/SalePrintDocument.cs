using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using SmartMed.BLL.Interfaces;

namespace SmartMed.UI.Components
{
    public class SalePrintDocument : PrintDocument
    {
        private readonly string _saleNumber;
        private readonly string _saleDate;
        private readonly string _cashier;
        private readonly string _customerName;
        private readonly string _customerPhone;
        private readonly List<PrintLineItem> _items;
        private readonly decimal _subTotal;
        private readonly decimal _discountPercent;
        private readonly decimal _taxPercent;
        private readonly decimal _grandTotal;
        private readonly decimal _amountPaid;
        private readonly decimal _balance;
        private readonly IPricingService _pricingService;

        private int _currentY;
        private int _pageWidth;
        private readonly Font _titleFont;
        private readonly Font _headerFont;
        private readonly Font _normalFont;
        private readonly Font _boldFont;
        private readonly Pen _linePen;
        private readonly Brush _textBrush;

        public SalePrintDocument(
            string saleNumber,
            string saleDate,
            string cashier,
            string customerName,
            string customerPhone,
            List<PrintLineItem> items,
            decimal subTotal,
            decimal discountPercent,
            decimal taxPercent,
            decimal grandTotal,
            decimal amountPaid,
            decimal balance,
            IPricingService pricingService)
        {
            _saleNumber = saleNumber;
            _saleDate = saleDate;
            _cashier = cashier;
            _customerName = customerName;
            _customerPhone = customerPhone;
            _items = items;
            _subTotal = subTotal;
            _discountPercent = discountPercent;
            _taxPercent = taxPercent;
            _grandTotal = grandTotal;
            _amountPaid = amountPaid;
            _balance = balance;
            _pricingService = pricingService;

            _titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            _headerFont = new Font("Segoe UI", 10, FontStyle.Bold);
            _normalFont = new Font("Segoe UI", 10);
            _boldFont = new Font("Segoe UI", 10, FontStyle.Bold);
            _linePen = new Pen(Color.Black, 1);
            _textBrush = Brushes.Black;

            PrintPage += OnPrintPage;
        }

        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            _pageWidth = (int)(e.MarginBounds.Width);
            _currentY = e.MarginBounds.Top;

            DrawHeader(e);
            DrawCustomerInfo(e);
            DrawLine(e);
            DrawColumnHeaders(e);
            DrawLine(e);
            DrawItems(e);
            DrawLine(e);
            DrawTotals(e);
            DrawLine(e);
            DrawFooter(e);
        }

        private void DrawHeader(PrintPageEventArgs e)
        {
            string title = "SMARTMED PHARMACY";
            SizeF titleSize = e.Graphics.MeasureString(title, _titleFont);
            float titleX = e.MarginBounds.Left + (_pageWidth - titleSize.Width) / 2;
            e.Graphics.DrawString(title, _titleFont, _textBrush, titleX, _currentY);

            _currentY += (int)titleSize.Height + 4;

            string subtitle = "SALES INVOICE";
            SizeF subSize = e.Graphics.MeasureString(subtitle, _headerFont);
            float subX = e.MarginBounds.Left + (_pageWidth - subSize.Width) / 2;
            e.Graphics.DrawString(subtitle, _headerFont, _textBrush, subX, _currentY);
            _currentY += (int)subSize.Height + 8;
        }

        private void DrawCustomerInfo(PrintPageEventArgs e)
        {
            e.Graphics.DrawString($"Sale #: {_saleNumber}", _normalFont, _textBrush, e.MarginBounds.Left, _currentY);
            _currentY += 18;
            e.Graphics.DrawString($"Date: {_saleDate}", _normalFont, _textBrush, e.MarginBounds.Left, _currentY);
            _currentY += 18;
            e.Graphics.DrawString($"Cashier: {_cashier}", _normalFont, _textBrush, e.MarginBounds.Left, _currentY);
            _currentY += 18;

            if (!string.IsNullOrWhiteSpace(_customerName))
            {
                e.Graphics.DrawString($"Customer: {_customerName}", _normalFont, _textBrush, e.MarginBounds.Left, _currentY);
                _currentY += 18;
            }

            if (!string.IsNullOrWhiteSpace(_customerPhone))
            {
                e.Graphics.DrawString($"Phone: {_customerPhone}", _normalFont, _textBrush, e.MarginBounds.Left, _currentY);
                _currentY += 18;
            }

            _currentY += 4;
        }

        private void DrawLine(PrintPageEventArgs e)
        {
            e.Graphics.DrawLine(_linePen, e.MarginBounds.Left, _currentY, e.MarginBounds.Right, _currentY);
            _currentY += 4;
        }

        private void DrawColumnHeaders(PrintPageEventArgs e)
        {
            string itemHeader = "Item";
            string qtyHeader = "Qty";
            string priceHeader = "Price";
            string discHeader = "Disc";
            string taxHeader = "Tax";
            string totalHeader = "Total";

            int[] colX = GetColumnPositions(e);
            e.Graphics.DrawString(itemHeader, _headerFont, _textBrush, colX[0], _currentY);
            e.Graphics.DrawString(qtyHeader, _headerFont, _textBrush, colX[1], _currentY);
            e.Graphics.DrawString(priceHeader, _headerFont, _textBrush, colX[2], _currentY);
            e.Graphics.DrawString(discHeader, _headerFont, _textBrush, colX[3], _currentY);
            e.Graphics.DrawString(taxHeader, _headerFont, _textBrush, colX[4], _currentY);
            e.Graphics.DrawString(totalHeader, _headerFont, _textBrush, colX[5], _currentY);
            _currentY += 20;
        }

        private void DrawItems(PrintPageEventArgs e)
        {
            int[] colX = GetColumnPositions(e);

            foreach (PrintLineItem item in _items)
            {
                string name = item.MedicineName.Length > 22 ? item.MedicineName.Substring(0, 22) : item.MedicineName;
                e.Graphics.DrawString(name, _normalFont, _textBrush, colX[0], _currentY);
                e.Graphics.DrawString(item.Quantity.ToString(), _normalFont, _textBrush, colX[1], _currentY);
                e.Graphics.DrawString(item.UnitPrice.ToString("N2"), _normalFont, _textBrush, colX[2], _currentY);
                e.Graphics.DrawString(item.DiscountPercent.ToString("N1") + "%", _normalFont, _textBrush, colX[3], _currentY);
                e.Graphics.DrawString(item.TaxPercent.ToString("N1") + "%", _normalFont, _textBrush, colX[4], _currentY);
                e.Graphics.DrawString(item.LineTotal.ToString("N2"), _normalFont, _textBrush, colX[5], _currentY);
                _currentY += 18;
            }

            _currentY += 4;
        }

        private void DrawTotals(PrintPageEventArgs e)
        {
            int labelX = e.MarginBounds.Left;
            int valueX = e.MarginBounds.Right - 100;

            e.Graphics.DrawString("Sub Total:", _normalFont, _textBrush, labelX, _currentY);
            e.Graphics.DrawString(_subTotal.ToString("N2"), _normalFont, _textBrush, valueX, _currentY);
            _currentY += 20;

            e.Graphics.DrawString($"Discount ({_discountPercent:N1}%):", _normalFont, _textBrush, labelX, _currentY);
            decimal discountAmount = _pricingService.CalculateDiscountAmount(_subTotal, _discountPercent);
            e.Graphics.DrawString($"-{discountAmount:N2}", _normalFont, _textBrush, valueX, _currentY);
            _currentY += 20;

            e.Graphics.DrawString($"Tax ({_taxPercent:N1}%):", _normalFont, _textBrush, labelX, _currentY);
            decimal taxAmount = _pricingService.CalculateTaxAmount(_subTotal - discountAmount, _taxPercent);
            e.Graphics.DrawString(taxAmount.ToString("N2"), _normalFont, _textBrush, valueX, _currentY);
            _currentY += 22;

            e.Graphics.DrawString("Grand Total:", _boldFont, _textBrush, labelX, _currentY);
            e.Graphics.DrawString(_grandTotal.ToString("N2"), _boldFont, _textBrush, valueX, _currentY);
            _currentY += 22;

            e.Graphics.DrawString("Amount Paid:", _normalFont, _textBrush, labelX, _currentY);
            e.Graphics.DrawString(_amountPaid.ToString("N2"), _normalFont, _textBrush, valueX, _currentY);
            _currentY += 20;

            e.Graphics.DrawString("Balance:", _normalFont, _textBrush, labelX, _currentY);
            e.Graphics.DrawString(_balance.ToString("N2"), _normalFont, _textBrush, valueX, _currentY);
            _currentY += 24;
        }

        private void DrawFooter(PrintPageEventArgs e)
        {
            string thankYou = "Thank you for your purchase!";
            SizeF footerSize = e.Graphics.MeasureString(thankYou, _boldFont);
            float footerX = e.MarginBounds.Left + (_pageWidth - footerSize.Width) / 2;
            e.Graphics.DrawString(thankYou, _boldFont, _textBrush, footerX, _currentY);
        }

        private static int[] GetColumnPositions(PrintPageEventArgs e)
        {
            int left = e.MarginBounds.Left;
            return new[]
            {
                left,
                left + 200,
                left + 250,
                left + 310,
                left + 360,
                left + 410
            };
        }
    }

    public class PrintLineItem
    {
        public string MedicineName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal LineTotal { get; set; }
    }
}
