using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartMed.UI.Theme.Controls
{
    public class SidebarItemClickedEventArgs : EventArgs
    {
        public string Key { get; }
        public SidebarItemClickedEventArgs(string key) { Key = key; }
    }

    /// <summary>
    /// A fixed-width dark navigation rail: brand header, a list of clickable
    /// icon+label items (with optional uppercase section dividers), and a
    /// bottom user identity block with a Logout action.
    /// </summary>
    public class SidebarNavControl : Panel
    {
        private class NavItem
        {
            public string Key;
            public char Glyph;
            public string Label;
            public Rectangle Bounds;
            public bool Visible = true;
            public bool IsSection;
        }

        private readonly List<NavItem> _items = new List<NavItem>();
        private string _activeKey;
        private string _hoveredKey;
        private string _displayName = string.Empty;
        private string _roleLabel = string.Empty;
        private Rectangle _logoutBounds;
        private bool _logoutHovered;

        public event EventHandler<SidebarItemClickedEventArgs> ItemClicked;
        public event EventHandler LogoutClicked;

        public const int SidebarWidth = 240;
        private const int ItemHeight = 44;
        private const int HeaderHeight = 72;
        private const int UserBlockHeight = 76;

        public SidebarNavControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Width = SidebarWidth;
            Dock = DockStyle.Left;
            BackColor = AppTheme.Sidebar;
            MouseMove += OnMouseMoveHandler;
            MouseLeave += (s, e) => { _hoveredKey = null; _logoutHovered = false; Invalidate(); };
            MouseClick += OnMouseClickHandler;
        }

        public void AddSection(string label)
        {
            _items.Add(new NavItem { Key = "__section_" + _items.Count, Label = label, IsSection = true });
            Invalidate();
        }

        public void AddItem(string key, char glyph, string label)
        {
            _items.Add(new NavItem { Key = key, Glyph = glyph, Label = label });
            Invalidate();
        }

        public void SetActive(string key)
        {
            _activeKey = key;
            Invalidate();
        }

        public void SetItemVisible(string key, bool visible)
        {
            NavItem item = _items.Find(i => i.Key == key);
            if (item != null)
            {
                item.Visible = visible;
                Invalidate();
            }
        }

        public void SetUserInfo(string displayName, string roleLabel)
        {
            _displayName = displayName ?? string.Empty;
            _roleLabel = roleLabel ?? string.Empty;
            Invalidate();
        }

        private void OnMouseMoveHandler(object sender, MouseEventArgs e)
        {
            string newHover = null;
            foreach (NavItem item in _items)
            {
                if (item.Visible && !item.IsSection && item.Bounds.Contains(e.Location))
                {
                    newHover = item.Key;
                    break;
                }
            }

            bool logoutHover = _logoutBounds.Contains(e.Location);

            if (newHover != _hoveredKey || logoutHover != _logoutHovered)
            {
                _hoveredKey = newHover;
                _logoutHovered = logoutHover;
                Invalidate();
            }
        }

        private void OnMouseClickHandler(object sender, MouseEventArgs e)
        {
            if (_logoutBounds.Contains(e.Location))
            {
                LogoutClicked?.Invoke(this, EventArgs.Empty);
                return;
            }

            foreach (NavItem item in _items)
            {
                if (item.Visible && !item.IsSection && item.Bounds.Contains(e.Location))
                {
                    ItemClicked?.Invoke(this, new SidebarItemClickedEventArgs(item.Key));
                    return;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(AppTheme.Sidebar);

            // Brand header
            using (SolidBrush accentBrush = new SolidBrush(AppTheme.Accent))
            {
                g.FillRectangle(accentBrush, 24, 24, 24, 8);
                g.FillRectangle(accentBrush, 32, 16, 8, 24);
            }
            using (SolidBrush textBrush = new SolidBrush(AppTheme.SidebarActiveText))
            using (Font wordmarkFont = new Font("Segoe UI", 13F, FontStyle.Bold))
            {
                g.DrawString("SmartMed", wordmarkFont, textBrush, new PointF(64, 20));
            }
            using (SolidBrush captionBrush = new SolidBrush(AppTheme.SidebarText))
            {
                g.DrawString("Pharmacy Suite", AppTheme.Caption, captionBrush, new PointF(64, 42));
            }

            int y = HeaderHeight;
            foreach (NavItem item in _items)
            {
                if (!item.Visible)
                {
                    item.Bounds = Rectangle.Empty;
                    continue;
                }

                if (item.IsSection)
                {
                    using (SolidBrush sectionBrush = new SolidBrush(AppTheme.SidebarText))
                    {
                        g.DrawString(item.Label.ToUpperInvariant(), AppTheme.Caption, sectionBrush, new PointF(24, y + 8));
                    }
                    y += 28;
                    continue;
                }

                item.Bounds = new Rectangle(0, y, Width, ItemHeight);
                bool isActive = item.Key == _activeKey;
                bool isHovered = item.Key == _hoveredKey;

                if (isActive)
                {
                    using (SolidBrush activeBrush = new SolidBrush(AppTheme.SidebarHover))
                        g.FillRectangle(activeBrush, item.Bounds);
                    using (SolidBrush barBrush = new SolidBrush(AppTheme.SidebarAccentBar))
                        g.FillRectangle(barBrush, 0, y, 3, ItemHeight);
                }
                else if (isHovered)
                {
                    using (SolidBrush hoverBrush = new SolidBrush(AppTheme.SidebarHover))
                        g.FillRectangle(hoverBrush, item.Bounds);
                }

                Color textColor = isActive ? AppTheme.SidebarActiveText : AppTheme.SidebarText;
                Bitmap icon = IconFactory.GetGlyph(item.Glyph, 18, textColor);
                g.DrawImage(icon, 24, y + (ItemHeight - 18) / 2, 18, 18);

                using (SolidBrush labelBrush = new SolidBrush(textColor))
                {
                    g.DrawString(item.Label, AppTheme.Body, labelBrush, new PointF(56, y + (ItemHeight - AppTheme.Body.Height) / 2));
                }

                y += ItemHeight;
            }

            // User block + logout, pinned to bottom
            int userBlockTop = Height - UserBlockHeight;
            using (Pen dividerPen = new Pen(AppTheme.SidebarHover, 1))
            {
                g.DrawLine(dividerPen, 0, userBlockTop, Width, userBlockTop);
            }

            int avatarSize = 32;
            Rectangle avatarBounds = new Rectangle(24, userBlockTop + 14, avatarSize, avatarSize);
            using (SolidBrush avatarBrush = new SolidBrush(AppTheme.Accent))
                g.FillEllipse(avatarBrush, avatarBounds);

            string initials = GetInitials(_displayName);
            using (SolidBrush initialsBrush = new SolidBrush(AppTheme.TextOnPrimary))
            using (StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(initials, AppTheme.CaptionBold, initialsBrush, avatarBounds, centerFormat);
            }

            using (SolidBrush nameBrush = new SolidBrush(AppTheme.SidebarActiveText))
            {
                g.DrawString(_displayName, AppTheme.BodyBold, nameBrush, new PointF(64, userBlockTop + 12));
            }
            using (SolidBrush roleBrush = new SolidBrush(AppTheme.SidebarText))
            {
                g.DrawString(_roleLabel, AppTheme.Caption, roleBrush, new PointF(64, userBlockTop + 30));
            }

            _logoutBounds = new Rectangle(0, userBlockTop + UserBlockHeight - 28, Width, 28);
            Color logoutColor = _logoutHovered ? AppTheme.Danger : AppTheme.SidebarText;
            Bitmap logoutIcon = IconFactory.GetGlyph(IconFactory.Logout, 14, logoutColor);
            g.DrawImage(logoutIcon, 24, _logoutBounds.Top + 7, 14, 14);
            using (SolidBrush logoutBrush = new SolidBrush(logoutColor))
            {
                g.DrawString("Logout", AppTheme.Caption, logoutBrush, new PointF(46, _logoutBounds.Top + 6));
            }
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant();
        }
    }
}
