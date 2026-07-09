using System.Drawing;

namespace SmartMed.UI.Theme
{
    /// <summary>
    /// Single source of truth for the application's design tokens (a shadcn-style
    /// zinc/slate palette adapted to WinForms). Every themed control reads its
    /// colors, fonts, and spacing from here rather than hard-coding values, so the
    /// whole application can be re-skinned by editing this one file.
    /// </summary>
    public static class AppTheme
    {
        // Surfaces
        public static readonly Color Background = ColorTranslator.FromHtml("#FAFAFA");
        public static readonly Color Surface = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color Border = ColorTranslator.FromHtml("#E4E4E7");
        public static readonly Color Divider = ColorTranslator.FromHtml("#F4F4F5");

        // Sidebar (Step 2+, defined now for reuse)
        public static readonly Color Sidebar = ColorTranslator.FromHtml("#18181B");
        public static readonly Color SidebarText = ColorTranslator.FromHtml("#A1A1AA");
        public static readonly Color SidebarHover = ColorTranslator.FromHtml("#27272A");
        public static readonly Color SidebarActiveText = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color SidebarAccentBar = ColorTranslator.FromHtml("#2563EB");

        // Primary / Accent
        public static readonly Color Primary = ColorTranslator.FromHtml("#18181B");
        public static readonly Color PrimaryHover = ColorTranslator.FromHtml("#27272A");
        public static readonly Color PrimaryPressed = ColorTranslator.FromHtml("#3F3F46");
        public static readonly Color Accent = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color TextOnPrimary = ColorTranslator.FromHtml("#FFFFFF");

        // Text
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#09090B");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#71717A");
        public static readonly Color TextDisabled = ColorTranslator.FromHtml("#A1A1AA");

        // Semantic
        public static readonly Color Success = ColorTranslator.FromHtml("#16A34A");
        public static readonly Color Warning = ColorTranslator.FromHtml("#D97706");
        public static readonly Color Danger = ColorTranslator.FromHtml("#DC2626");
        public static readonly Color Info = ColorTranslator.FromHtml("#2563EB");

        // Order status (Step 3+, defined now for reuse)
        public static readonly Color StatusPending = ColorTranslator.FromHtml("#D97706");
        public static readonly Color StatusReadyForPickup = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color StatusDelivered = ColorTranslator.FromHtml("#16A34A");
        public static readonly Color StatusCancelled = ColorTranslator.FromHtml("#71717A");

        // Typography
        private const string FontFamily = "Segoe UI";

        public static readonly Font PageTitle = new Font(FontFamily, 20F, FontStyle.Bold);
        public static readonly Font SectionHeader = new Font(FontFamily, 13F, FontStyle.Bold);
        public static readonly Font Body = new Font(FontFamily, 9.75F, FontStyle.Regular);
        public static readonly Font BodyBold = new Font(FontFamily, 9.75F, FontStyle.Bold);
        public static readonly Font Caption = new Font(FontFamily, 8.25F, FontStyle.Regular);
        public static readonly Font CaptionBold = new Font(FontFamily, 8.25F, FontStyle.Bold);

        // Spacing scale
        public const int Space1 = 4;
        public const int Space2 = 8;
        public const int Space3 = 12;
        public const int Space4 = 16;
        public const int Space6 = 24;
        public const int Space8 = 32;

        public const int CornerRadius = 8;
        public const int PagePadding = 24;
        public const int ControlHeight = 40;
    }
}
