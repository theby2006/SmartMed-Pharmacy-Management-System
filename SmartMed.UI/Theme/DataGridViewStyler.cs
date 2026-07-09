using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace SmartMed.UI.Theme
{
    /// <summary>
    /// Applies the application's grid visual language to any
    /// <see cref="DataGridView"/> in one call: no row headers, a light
    /// uppercase-caption header band, 44px rows, hairline horizontal
    /// gridlines only, and hover/selection tinting. Not yet applied to any
    /// form in this step (Step 1 restyles only <c>LoginForm</c>, which has
    /// no grid); wired up starting in Step 4 when the grid-based staff forms
    /// are restyled.
    /// </summary>
    public static class DataGridViewStyler
    {
        public static void Apply(DataGridView grid)
        {
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
            grid.GridColor = AppTheme.Divider;
            grid.BackgroundColor = AppTheme.Surface;
            grid.EnableHeadersVisualStyles = false;
            grid.RowTemplate.Height = 44;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;

            grid.ColumnHeadersHeight = 40;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Background;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextSecondary;
            grid.ColumnHeadersDefaultCellStyle.Font = AppTheme.CaptionBold;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 0, 0);
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            grid.DefaultCellStyle.BackColor = AppTheme.Surface;
            grid.DefaultCellStyle.ForeColor = AppTheme.TextPrimary;
            grid.DefaultCellStyle.Font = AppTheme.Body;
            grid.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#EFF6FF");
            grid.DefaultCellStyle.SelectionForeColor = AppTheme.TextPrimary;
            grid.DefaultCellStyle.Padding = new Padding(12, 0, 0, 0);

            grid.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.Surface;

            grid.CellMouseEnter -= OnCellMouseEnter;
            grid.CellMouseLeave -= OnCellMouseLeave;
            grid.CellMouseEnter += OnCellMouseEnter;
            grid.CellMouseLeave += OnCellMouseLeave;

            EnableDoubleBuffering(grid);
        }

        private static void OnCellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridView grid = (DataGridView)sender;
            if (!grid.Rows[e.RowIndex].Selected)
                grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = AppTheme.Divider;
        }

        private static void OnCellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridView grid = (DataGridView)sender;
            if (!grid.Rows[e.RowIndex].Selected)
                grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = AppTheme.Surface;
        }

        private static void EnableDoubleBuffering(DataGridView grid)
        {
            PropertyInfo property = typeof(DataGridView).GetProperty(
                "DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            property?.SetValue(grid, true, null);
        }
    }
}
