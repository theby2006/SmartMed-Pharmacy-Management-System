using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartMed.UI.Theme.Controls
{
    /// <summary>
    /// A themed text input: a borderless inner <see cref="TextBox"/> hosted
    /// inside a rounded, owner-drawn container that reflects focus and
    /// validation-error states, with optional leading icon, placeholder text,
    /// and a password-reveal toggle.
    /// </summary>
    public class ModernTextBox : UserControl
    {
        private readonly TextBox _innerTextBox;
        private readonly Label _errorLabel;
        private char? _leadingIcon;
        private bool _isPasswordField;
        private bool _revealPassword;
        private string _placeholderText = string.Empty;
        private bool _hasError;
        private Rectangle _eyeToggleBounds;

        public ModernTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Height = AppTheme.ControlHeight + 22;
            BackColor = Parent?.BackColor ?? AppTheme.Surface;

            _innerTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = AppTheme.Body,
                ForeColor = AppTheme.TextPrimary,
                BackColor = AppTheme.Surface
            };
            _innerTextBox.GotFocus += (s, e) => Invalidate();
            _innerTextBox.LostFocus += (s, e) => Invalidate();
            _innerTextBox.TextChanged += (s, e) =>
            {
                Invalidate();
                OnTextChanged(EventArgs.Empty);
            };
            Controls.Add(_innerTextBox);

            _errorLabel = new Label
            {
                AutoSize = false,
                Font = AppTheme.Caption,
                ForeColor = AppTheme.Danger,
                Dock = DockStyle.Bottom,
                Height = 16,
                Visible = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(_errorLabel);

            Resize += (s, e) => LayoutInner();
        }

        public string PlaceholderText
        {
            get => _placeholderText;
            set { _placeholderText = value ?? string.Empty; Invalidate(); }
        }

        public char? LeadingIcon
        {
            get => _leadingIcon;
            set { _leadingIcon = value; LayoutInner(); Invalidate(); }
        }

        public bool IsPasswordField
        {
            get => _isPasswordField;
            set
            {
                _isPasswordField = value;
                _innerTextBox.UseSystemPasswordChar = value && !_revealPassword;
                LayoutInner();
                Invalidate();
            }
        }

        public override string Text
        {
            get => _innerTextBox.Text;
            set => _innerTextBox.Text = value;
        }

        public void SetError(string message)
        {
            _hasError = true;
            _errorLabel.Text = message;
            _errorLabel.Visible = !string.IsNullOrEmpty(message);
            Height = AppTheme.ControlHeight + (_errorLabel.Visible ? 22 : 4);
            Invalidate();
        }

        public void ClearError()
        {
            _hasError = false;
            _errorLabel.Visible = false;
            Height = AppTheme.ControlHeight + 4;
            Invalidate();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LayoutInner();
        }

        private void LayoutInner()
        {
            int left = AppTheme.Space3 + (_leadingIcon.HasValue ? 24 : 0);
            int right = AppTheme.Space3 + (_isPasswordField ? 24 : 0);
            int fieldHeight = AppTheme.ControlHeight;

            _innerTextBox.Location = new Point(left, (fieldHeight - _innerTextBox.Height) / 2);
            _innerTextBox.Width = Math.Max(10, Width - left - right);

            if (_isPasswordField)
            {
                _eyeToggleBounds = new Rectangle(Width - AppTheme.Space3 - 18, (fieldHeight - 18) / 2, 18, 18);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_isPasswordField && _eyeToggleBounds.Contains(e.Location))
            {
                _revealPassword = !_revealPassword;
                _innerTextBox.UseSystemPasswordChar = !_revealPassword;
                Invalidate();
            }
            else
            {
                _innerTextBox.Focus();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? AppTheme.Background);

            int fieldHeight = AppTheme.ControlHeight;
            Rectangle bounds = new Rectangle(0, 0, Width - 1, fieldHeight - 1);

            Color borderColor = _hasError ? AppTheme.Danger : (_innerTextBox.Focused ? AppTheme.Accent : AppTheme.Border);
            int borderWidth = (_innerTextBox.Focused || _hasError) ? 2 : 1;

            using (GraphicsPath path = IconFactory.RoundedRectanglePath(bounds, AppTheme.CornerRadius))
            {
                using (SolidBrush brush = new SolidBrush(AppTheme.Surface))
                    g.FillPath(brush, path);

                using (Pen pen = new Pen(borderColor, borderWidth))
                    g.DrawPath(pen, path);
            }

            if (_leadingIcon.HasValue)
            {
                int iconSize = 16;
                Bitmap icon = IconFactory.GetGlyph(_leadingIcon.Value, iconSize, AppTheme.TextSecondary);
                g.DrawImage(icon, AppTheme.Space3, (fieldHeight - iconSize) / 2, iconSize, iconSize);
            }

            if (string.IsNullOrEmpty(_innerTextBox.Text) && !_innerTextBox.Focused && !string.IsNullOrEmpty(_placeholderText))
            {
                using (SolidBrush placeholderBrush = new SolidBrush(AppTheme.TextDisabled))
                {
                    g.DrawString(_placeholderText, AppTheme.Body, placeholderBrush,
                        new PointF(_innerTextBox.Left, (fieldHeight - _innerTextBox.Height) / 2 + 2));
                }
            }

            if (_isPasswordField)
            {
                Color eyeColor = _revealPassword ? AppTheme.Accent : AppTheme.TextSecondary;
                Bitmap icon = IconFactory.GetGlyph(IconFactory.Eye, 16, eyeColor);
                g.DrawImage(icon, _eyeToggleBounds.Left, _eyeToggleBounds.Top, 16, 16);
            }
        }
    }
}
