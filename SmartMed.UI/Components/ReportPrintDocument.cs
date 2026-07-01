using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using SmartMed.Models.Reports;

namespace SmartMed.UI.Components
{
    public class ReportPrintDocument : PrintDocument
    {
        private readonly string _title;
        private readonly string _dateRange;
        private readonly List<string[]> _rows;
        private readonly string[] _headers;
        private readonly int[] _columnWidths;

        private int _currentY;
        private int _pageWidth;
        private readonly Font _titleFont;
        private readonly Font _headerFont;
        private readonly Font _normalFont;
        private readonly Font _boldFont;
        private readonly Pen _linePen;
        private readonly Brush _textBrush;

        public ReportPrintDocument(
            string title,
            string dateRange,
            string[] headers,
            List<string[]> rows,
            int[] columnWidths = null)
        {
            _title = title;
            _dateRange = dateRange;
            _headers = headers;
            _rows = rows;
            _columnWidths = columnWidths;

            _titleFont = new Font("Segoe UI", 14, FontStyle.Bold);
            _headerFont = new Font("Segoe UI", 9, FontStyle.Bold);
            _normalFont = new Font("Segoe UI", 9);
            _boldFont = new Font("Segoe UI", 9, FontStyle.Bold);
            _linePen = new Pen(Color.Black, 1);
            _textBrush = Brushes.Black;

            PrintPage += OnPrintPage;
        }

        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            _pageWidth = e.MarginBounds.Width;
            _currentY = e.MarginBounds.Top;

            DrawTitle(e);
            DrawDateRange(e);
            DrawLine(e);
            DrawColumnHeaders(e);
            DrawLine(e);
            DrawRows(e);
            DrawLine(e);
            DrawFooter(e);
        }

        private void DrawTitle(PrintPageEventArgs e)
        {
            SizeF titleSize = e.Graphics.MeasureString(_title, _titleFont);
            float titleX = e.MarginBounds.Left + (_pageWidth - titleSize.Width) / 2;
            e.Graphics.DrawString(_title, _titleFont, _textBrush, titleX, _currentY);
            _currentY += (int)titleSize.Height + 6;
        }

        private void DrawDateRange(PrintPageEventArgs e)
        {
            e.Graphics.DrawString(_dateRange, _normalFont, _textBrush, e.MarginBounds.Left, _currentY);
            _currentY += 18;
        }

        private void DrawLine(PrintPageEventArgs e)
        {
            e.Graphics.DrawLine(_linePen, e.MarginBounds.Left, _currentY, e.MarginBounds.Right, _currentY);
            _currentY += 4;
        }

        private void DrawColumnHeaders(PrintPageEventArgs e)
        {
            int[] colX = GetColumnPositions(e);
            for (int i = 0; i < _headers.Length; i++)
            {
                e.Graphics.DrawString(_headers[i], _headerFont, _textBrush, colX[i], _currentY);
            }
            _currentY += 18;
        }

        private void DrawRows(PrintPageEventArgs e)
        {
            int[] colX = GetColumnPositions(e);

            foreach (string[] row in _rows)
            {
                if (_currentY > e.MarginBounds.Bottom - 20)
                {
                    e.HasMorePages = true;
                    return;
                }

                for (int i = 0; i < row.Length && i < colX.Length; i++)
                {
                    e.Graphics.DrawString(row[i], _normalFont, _textBrush, colX[i], _currentY);
                }
                _currentY += 16;
            }
        }

        private void DrawFooter(PrintPageEventArgs e)
        {
            string footer = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}";
            e.Graphics.DrawString(footer, _normalFont, _textBrush, e.MarginBounds.Left, _currentY);
        }

        private int[] GetColumnPositions(PrintPageEventArgs e)
        {
            if (_columnWidths != null && _columnWidths.Length > 0)
            {
                int left = e.MarginBounds.Left;
                int[] positions = new int[_columnWidths.Length];
                int current = left;
                for (int i = 0; i < _columnWidths.Length; i++)
                {
                    positions[i] = current;
                    current += _columnWidths[i];
                }
                return positions;
            }

            int colCount = _headers.Length;
            int colWidth = _pageWidth / Math.Max(colCount, 1);
            int[] defaultPositions = new int[colCount];
            for (int i = 0; i < colCount; i++)
                defaultPositions[i] = e.MarginBounds.Left + (i * colWidth);

            return defaultPositions;
        }
    }
}
