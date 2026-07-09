using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SmartMed.Models.Enums;

namespace SmartMed.UI.Theme.Controls
{
    /// <summary>
    /// A pill-shaped label tinted with a semantic color at low alpha, used to
    /// show order status, promotion flags, and stock state at a glance.
    /// </summary>
    public class StatusBadge : Label
    {
        private Color _accentColor = AppTheme.Info;

        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        public StatusBadge()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            AutoSize = true;
            Font = AppTheme.CaptionBold;
            Padding = new Padding(10, 4, 10, 4);
            BackColor = Color.Transparent;
        }

        public void SetStatus(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Pending:
                    AccentColor = AppTheme.StatusPending;
                    Text = "Pending";
                    break;
                case OrderStatus.PrescriptionReviewRequired:
                    AccentColor = AppTheme.StatusPending;
                    Text = "Awaiting Prescription";
                    break;
                case OrderStatus.Approved:
                    AccentColor = AppTheme.StatusReadyForPickup;
                    Text = "Approved";
                    break;
                case OrderStatus.Processing:
                    AccentColor = AppTheme.StatusReadyForPickup;
                    Text = "Ready for Pickup";
                    break;
                case OrderStatus.Completed:
                    AccentColor = AppTheme.StatusDelivered;
                    Text = "Delivered";
                    break;
                case OrderStatus.Cancelled:
                    AccentColor = AppTheme.StatusCancelled;
                    Text = "Cancelled";
                    break;
                case OrderStatus.Rejected:
                    AccentColor = AppTheme.Danger;
                    Text = "Rejected";
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? AppTheme.Surface);

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = IconFactory.RoundedRectanglePath(bounds, Height / 2))
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(38, _accentColor.R, _accentColor.G, _accentColor.B)))
                    g.FillPath(brush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(_accentColor))
            using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(Text, Font, textBrush, ClientRectangle, format);
            }
        }
    }
}
