using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartMed.UI.Theme.Controls
{
    /// <summary>
    /// A white, rounded-corner surface with a subtle layered drop shadow,
    /// used as the base "card" container for forms, dialogs, and content
    /// blocks throughout the themed UI.
    /// </summary>
    public class CardPanel : Panel
    {
        public int CornerRadius { get; set; } = AppTheme.CornerRadius;

        public CardPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            Padding = new Padding(AppTheme.Space6);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? AppTheme.Background);

            Rectangle shadowBounds = new Rectangle(2, 3, Width - 5, Height - 5);
            for (int i = 3; i >= 1; i--)
            {
                using (GraphicsPath shadowPath = IconFactory.RoundedRectanglePath(
                    Rectangle.Inflate(shadowBounds, i, i), CornerRadius + i))
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(6, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = IconFactory.RoundedRectanglePath(bounds, CornerRadius))
            {
                using (SolidBrush fillBrush = new SolidBrush(AppTheme.Surface))
                    g.FillPath(fillBrush, path);

                using (Pen borderPen = new Pen(AppTheme.Border, 1))
                    g.DrawPath(borderPen, path);
            }
        }
    }
}
