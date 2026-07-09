using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace SmartMed.UI.Theme
{
    /// <summary>
    /// Renders icon glyphs from the "Segoe MDL2 Assets" font that ships with
    /// Windows, so the application needs zero image asset files. Glyphs are
    /// rasterized to <see cref="Bitmap"/> on first request and cached by
    /// (glyph, size, color); the application's window icon is likewise
    /// synthesized in memory rather than shipped as a static .ico file.
    /// </summary>
    public static class IconFactory
    {
        private const string GlyphFontFamily = "Segoe MDL2 Assets";

        public const char Dashboard = '';
        public const char Medicine = '';
        public const char Category = '';
        public const char Cart = '';
        public const char Orders = '';
        public const char Customers = '';
        public const char Suppliers = '';
        public const char Purchases = '';
        public const char Reports = '';
        public const char Sales = '';
        public const char Settings = '';
        public const char Search = '';
        public const char Add = '';
        public const char Edit = '';
        public const char Delete = '';
        public const char Save = '';
        public const char Refresh = '';
        public const char Logout = '';
        public const char Warning = '';
        public const char Bell = '';
        public const char Check = '';
        public const char Close = '';
        public const char ChevronRight = '';
        public const char Upload = '';
        public const char Pdf = '';
        public const char Excel = '';
        public const char User = '';
        public const char Lock = '';
        public const char Eye = '';

        private static readonly Dictionary<(char Glyph, int Size, int Argb), Bitmap> Cache =
            new Dictionary<(char, int, int), Bitmap>();

        /// <summary>
        /// Returns a cached, transparent-background bitmap of the given glyph
        /// rendered at approximately <paramref name="size"/> pixels square.
        /// </summary>
        public static Bitmap GetGlyph(char glyph, int size, Color color)
        {
            var key = (glyph, size, color.ToArgb());
            if (Cache.TryGetValue(key, out Bitmap cached))
                return cached;

            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            using (Font font = new Font(GlyphFontFamily, size * 0.62F, FontStyle.Regular, GraphicsUnit.Pixel))
            using (SolidBrush brush = new SolidBrush(color))
            using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.Clear(Color.Transparent);
                g.DrawString(glyph.ToString(), font, brush, new RectangleF(0, 0, size, size), format);
            }

            Cache[key] = bitmap;
            return bitmap;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Builds the application window icon entirely in memory: a rounded
        /// dark square (<see cref="AppTheme.Primary"/>) with a white "S"
        /// wordmark and a small plus-shaped accent mark, evoking a pharmacy
        /// cross without requiring an external image file.
        /// </summary>
        public static Icon BuildAppIcon(int size = 256)
        {
            using (Bitmap bitmap = new Bitmap(size, size))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                int radius = size / 5;
                Rectangle bounds = new Rectangle(0, 0, size - 1, size - 1);
                using (GraphicsPath path = RoundedRectanglePath(bounds, radius))
                using (SolidBrush backgroundBrush = new SolidBrush(AppTheme.Primary))
                {
                    g.FillPath(backgroundBrush, path);
                }

                using (Font sFont = new Font("Segoe UI", size * 0.5F, FontStyle.Bold, GraphicsUnit.Pixel))
                using (SolidBrush textBrush = new SolidBrush(AppTheme.TextOnPrimary))
                using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    RectangleF textArea = new RectangleF(0, -size * 0.04F, size, size);
                    g.DrawString("S", sFont, textBrush, textArea, format);
                }

                // Small plus/cross accent, bottom-right, evoking a pharmacy cross.
                int crossSize = size / 6;
                int crossThickness = Math.Max(2, crossSize / 4);
                int crossCenterX = (int)(size * 0.78);
                int crossCenterY = (int)(size * 0.78);
                using (SolidBrush accentBrush = new SolidBrush(AppTheme.Accent))
                {
                    g.FillRectangle(accentBrush,
                        crossCenterX - crossSize / 2, crossCenterY - crossThickness / 2,
                        crossSize, crossThickness);
                    g.FillRectangle(accentBrush,
                        crossCenterX - crossThickness / 2, crossCenterY - crossSize / 2,
                        crossThickness, crossSize);
                }

                IntPtr hIcon = bitmap.GetHicon();
                try
                {
                    using (Icon temp = Icon.FromHandle(hIcon))
                    {
                        return (Icon)temp.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
        }

        internal static GraphicsPath RoundedRectanglePath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
