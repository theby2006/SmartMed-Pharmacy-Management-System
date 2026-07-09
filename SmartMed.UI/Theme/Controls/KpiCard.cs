using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartMed.UI.Theme.Controls
{
    /// <summary>
    /// A dashboard summary tile: an icon in a tinted rounded square, a large
    /// metric value, and a caption. Built on <see cref="CardPanel"/>.
    /// </summary>
    public class KpiCard : CardPanel
    {
        private char _glyph = IconFactory.Dashboard;
        private string _metricValue = "0";
        private string _caption = string.Empty;
        private Color _accentColor = AppTheme.Accent;

        public char Glyph
        {
            get => _glyph;
            set { _glyph = value; Invalidate(); }
        }

        public string MetricValue
        {
            get => _metricValue;
            set { _metricValue = value ?? string.Empty; Invalidate(); }
        }

        public string Caption
        {
            get => _caption;
            set { _caption = value ?? string.Empty; Invalidate(); }
        }

        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        public KpiCard()
        {
            Size = new Size(220, 110);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int iconBoxSize = 40;
            int left = AppTheme.Space4;
            int top = AppTheme.Space4;

            Rectangle iconBox = new Rectangle(left, top, iconBoxSize, iconBoxSize);
            using (GraphicsPath path = IconFactory.RoundedRectanglePath(iconBox, 10))
            using (SolidBrush tintBrush = new SolidBrush(Color.FromArgb(30, _accentColor.R, _accentColor.G, _accentColor.B)))
            {
                g.FillPath(tintBrush, path);
            }

            Bitmap icon = IconFactory.GetGlyph(_glyph, 20, _accentColor);
            g.DrawImage(icon, left + (iconBoxSize - 20) / 2, top + (iconBoxSize - 20) / 2, 20, 20);

            int textTop = top + iconBoxSize + AppTheme.Space3;
            using (SolidBrush valueBrush = new SolidBrush(AppTheme.TextPrimary))
            {
                g.DrawString(_metricValue, AppTheme.PageTitle, valueBrush, new PointF(left, textTop));
            }

            SizeF valueSize = g.MeasureString(_metricValue, AppTheme.PageTitle);
            using (SolidBrush captionBrush = new SolidBrush(AppTheme.TextSecondary))
            {
                g.DrawString(_caption, AppTheme.Caption, captionBrush, new PointF(left, textTop + valueSize.Height));
            }
        }
    }
}
