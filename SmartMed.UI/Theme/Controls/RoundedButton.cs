using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartMed.UI.Theme.Controls
{
    public enum ButtonVariant
    {
        Primary,
        Secondary,
        Outline,
        Ghost,
        Danger
    }

    /// <summary>
    /// Owner-drawn button implementing the theme's five visual variants, with
    /// hover/pressed color shifts, an optional leading icon, and a rounded
    /// pill/rect shape. Replaces every default <see cref="Button"/> in themed
    /// forms.
    /// </summary>
    public class RoundedButton : Button
    {
        private bool _isHovered;
        private bool _isPressed;
        private char? _iconGlyph;

        public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;

        public char? IconGlyph
        {
            get => _iconGlyph;
            set { _iconGlyph = value; Invalidate(); }
        }

        public RoundedButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            BackColor = Color.Transparent;
            Font = AppTheme.BodyBold;
            Height = AppTheme.ControlHeight;
            Cursor = Cursors.Hand;
            TabStop = true;
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            Invalidate();
        }

        private (Color background, Color foreground, Color border) ResolveColors()
        {
            if (!Enabled)
            {
                return Variant switch
                {
                    ButtonVariant.Outline => (AppTheme.Surface, AppTheme.TextDisabled, AppTheme.Border),
                    ButtonVariant.Ghost => (Color.Transparent, AppTheme.TextDisabled, Color.Transparent),
                    _ => (ControlPaint.Light(AppTheme.Primary, 0.6F), Color.White, Color.Transparent)
                };
            }

            switch (Variant)
            {
                case ButtonVariant.Primary:
                    Color primaryBg = _isPressed ? AppTheme.PrimaryPressed : (_isHovered ? AppTheme.PrimaryHover : AppTheme.Primary);
                    return (primaryBg, AppTheme.TextOnPrimary, Color.Transparent);

                case ButtonVariant.Danger:
                    Color dangerBg = _isPressed ? ControlPaint.Dark(AppTheme.Danger, 0.1F) : (_isHovered ? ControlPaint.Light(AppTheme.Danger, 0.1F) : AppTheme.Danger);
                    return (dangerBg, Color.White, Color.Transparent);

                case ButtonVariant.Secondary:
                    Color secondaryBg = _isHovered ? AppTheme.Divider : AppTheme.Border;
                    return (secondaryBg, AppTheme.TextPrimary, Color.Transparent);

                case ButtonVariant.Outline:
                    Color outlineBg = _isHovered ? AppTheme.Divider : AppTheme.Surface;
                    return (outlineBg, AppTheme.TextPrimary, AppTheme.Border);

                case ButtonVariant.Ghost:
                default:
                    Color ghostBg = _isHovered ? AppTheme.Divider : Color.Transparent;
                    return (ghostBg, AppTheme.TextSecondary, Color.Transparent);
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor.A == 0 ? Parent?.BackColor ?? SystemColors.Control : BackColor);

            (Color background, Color foreground, Color border) = ResolveColors();

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = IconFactory.RoundedRectanglePath(bounds, AppTheme.CornerRadius))
            {
                using (SolidBrush brush = new SolidBrush(background))
                    g.FillPath(brush, path);

                if (border != Color.Transparent)
                {
                    using (Pen pen = new Pen(border, 1))
                        g.DrawPath(pen, path);
                }
            }

            int contentLeft = AppTheme.Space4;
            if (_iconGlyph.HasValue)
            {
                int iconSize = 16;
                Bitmap icon = IconFactory.GetGlyph(_iconGlyph.Value, iconSize, foreground);
                g.DrawImage(icon, contentLeft, (Height - iconSize) / 2, iconSize, iconSize);
                contentLeft += iconSize + AppTheme.Space2;
            }

            Rectangle textRect = new Rectangle(contentLeft, 0, Width - contentLeft - AppTheme.Space4, Height);
            using (SolidBrush textBrush = new SolidBrush(foreground))
            using (StringFormat format = new StringFormat { Alignment = _iconGlyph.HasValue ? StringAlignment.Near : StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(Text, Font, textBrush, textRect, format);
            }

            if (Focused && ShowFocusCues)
            {
                Rectangle focusRect = Rectangle.Inflate(bounds, -3, -3);
                using (Pen focusPen = new Pen(AppTheme.Accent, 1) { DashStyle = DashStyle.Dot })
                    g.DrawRectangle(focusPen, focusRect);
            }
        }
    }
}
