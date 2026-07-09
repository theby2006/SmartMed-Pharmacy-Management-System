using System.Drawing;
using System.Windows.Forms;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Theme
{
    public enum ToastKind
    {
        Success,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Shows a brief, auto-dismissing confirmation strip anchored to the
    /// top-right of a host form, replacing success/info <see cref="MessageBox"/>
    /// popups per the theme's "no non-destructive MessageBox" rule.
    /// </summary>
    public static class ToastNotifier
    {
        public static void Show(Form host, string message, ToastKind kind = ToastKind.Success)
        {
            if (host == null || host.IsDisposed) return;

            (Color accent, char glyph) = kind switch
            {
                ToastKind.Success => (AppTheme.Success, IconFactory.Check),
                ToastKind.Warning => (AppTheme.Warning, IconFactory.Warning),
                ToastKind.Error => (AppTheme.Danger, IconFactory.Close),
                _ => (AppTheme.Info, IconFactory.Bell)
            };

            CardPanel toast = new CardPanel
            {
                Size = new Size(320, 56),
                BackColor = Color.Transparent
            };
            toast.Location = new Point(host.ClientSize.Width - toast.Width - AppTheme.Space6, AppTheme.Space6);
            toast.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            PictureBox iconBox = new PictureBox
            {
                Image = IconFactory.GetGlyph(glyph, 18, accent),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Size = new Size(18, 18),
                Location = new Point(16, 19),
                BackColor = Color.Transparent
            };
            toast.Controls.Add(iconBox);

            Label messageLabel = new Label
            {
                Text = message,
                Font = AppTheme.Body,
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(44, 0),
                Size = new Size(toast.Width - 60, toast.Height),
                TextAlign = ContentAlignment.MiddleLeft
            };
            toast.Controls.Add(messageLabel);

            host.Controls.Add(toast);
            toast.BringToFront();

            Timer dismissTimer = new Timer { Interval = 3000 };
            dismissTimer.Tick += (s, e) =>
            {
                dismissTimer.Stop();
                dismissTimer.Dispose();
                host.Controls.Remove(toast);
                toast.Dispose();
            };
            dismissTimer.Start();
        }
    }
}
