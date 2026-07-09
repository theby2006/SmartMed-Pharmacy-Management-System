using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Models;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;
using SmartMed.UI.Services;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    /// <summary>
    /// Customer-facing medicine catalogue browser. All filtering is done by
    /// <see cref="IMedicineSearchService"/> against an in-memory snapshot of
    /// the catalogue rather than a fresh SQL query per keystroke, so the
    /// linear-search/filter/binary-search algorithms built for this system
    /// are the ones actually driving this screen.
    /// </summary>
    public class BrowseMedicinesForm : Form
    {
        private readonly IMedicineService _medicineService;
        private readonly IMedicineCategoryService _categoryService;
        private readonly IMedicineSearchService _searchService;
        private readonly CartService _cartService;

        private ModernTextBox txtSearch;
        private ComboBox cboCategory;
        private ModernTextBox txtMinPrice;
        private ModernTextBox txtMaxPrice;
        private RoundedButton btnSearch;
        private RoundedButton btnClear;
        private DataGridView dgvResults;
        private Label lblResultCaption;
        private NumericUpDown nudQuantity;
        private RoundedButton btnAddToCart;

        private List<MedicineCategory> _categories = new List<MedicineCategory>();
        private List<Medicine> _allMedicines = new List<Medicine>();
        private List<Medicine> _currentResults = new List<Medicine>();

        public BrowseMedicinesForm(
            IMedicineService medicineService,
            IMedicineCategoryService categoryService,
            IMedicineSearchService searchService,
            CartService cartService)
        {
            _medicineService = medicineService;
            _categoryService = categoryService;
            _searchService = searchService;
            _cartService = cartService;
            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            Text = "Browse Medicines";
            BackColor = AppTheme.Background;

            Label title = new Label
            {
                AutoSize = true,
                Font = AppTheme.PageTitle,
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(0, 0),
                Text = "Browse Medicines"
            };
            Controls.Add(title);

            int toolbarY = 48;
            txtSearch = new ModernTextBox
            {
                Location = new Point(0, toolbarY),
                Width = 260,
                PlaceholderText = "Search by name or brand",
                LeadingIcon = IconFactory.Search
            };
            Controls.Add(txtSearch);

            cboCategory = new ComboBox
            {
                Font = AppTheme.Body,
                Location = new Point(272, toolbarY + 10),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(cboCategory);

            txtMinPrice = new ModernTextBox { Location = new Point(444, toolbarY), Width = 90, PlaceholderText = "Min $" };
            Controls.Add(txtMinPrice);

            txtMaxPrice = new ModernTextBox { Location = new Point(544, toolbarY), Width = 90, PlaceholderText = "Max $" };
            Controls.Add(txtMaxPrice);

            btnSearch = new RoundedButton { Variant = ButtonVariant.Primary, Text = "Search", IconGlyph = IconFactory.Search, Location = new Point(644, toolbarY), Width = 110 };
            btnSearch.Click += (s, e) => ApplySearch();
            Controls.Add(btnSearch);

            btnClear = new RoundedButton { Variant = ButtonVariant.Ghost, Text = "Clear", Location = new Point(760, toolbarY), Width = 90 };
            btnClear.Click += (s, e) => ClearFilters();
            Controls.Add(btnClear);

            lblResultCaption = new Label
            {
                AutoSize = true,
                Font = AppTheme.Caption,
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(0, toolbarY + 48),
                Text = ""
            };
            Controls.Add(lblResultCaption);

            dgvResults = new DataGridView
            {
                Location = new Point(0, toolbarY + 72),
                Size = new Size(900, 360),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            DataGridViewStyler.Apply(dgvResults);
            dgvResults.SelectionChanged += (s, e) => UpdateAddToCartState();
            Controls.Add(dgvResults);

            int actionY = toolbarY + 72 + 372;
            Label lblQty = new Label { AutoSize = true, Font = AppTheme.Body, Text = "Qty:", Location = new Point(0, actionY + 8) };
            Controls.Add(lblQty);

            nudQuantity = new NumericUpDown { Minimum = 1, Maximum = 999, Value = 1, Location = new Point(40, actionY), Width = 70, Font = AppTheme.Body };
            Controls.Add(nudQuantity);

            btnAddToCart = new RoundedButton { Variant = ButtonVariant.Primary, Text = "Add to Cart", IconGlyph = IconFactory.Cart, Location = new Point(120, actionY), Width = 150 };
            btnAddToCart.Click += BtnAddToCart_Click;
            Controls.Add(btnAddToCart);
        }

        private void LoadData()
        {
            OperationResult<List<MedicineCategory>> categoryResult = _categoryService.GetAllCategories();
            _categories = categoryResult.IsSuccess ? categoryResult.Data : new List<MedicineCategory>();

            cboCategory.Items.Clear();
            cboCategory.Items.Add("All categories");
            foreach (MedicineCategory category in _categories)
                cboCategory.Items.Add(category.Name);
            cboCategory.SelectedIndex = 0;

            OperationResult<List<Medicine>> medicinesResult = _medicineService.GetAllMedicines();
            _allMedicines = medicinesResult.IsSuccess ? medicinesResult.Data : new List<Medicine>();

            BindResults(_allMedicines, "linear scan (no filters applied)");
        }

        private void ApplySearch()
        {
            DateTime start = DateTime.UtcNow;

            MedicineSearchCriteria criteria = new MedicineSearchCriteria
            {
                NameTerm = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim()
            };

            if (cboCategory.SelectedIndex > 0)
                criteria.CategoryId = _categories[cboCategory.SelectedIndex - 1].Id;

            if (decimal.TryParse(txtMinPrice.Text, out decimal min))
                criteria.MinPrice = min;

            if (decimal.TryParse(txtMaxPrice.Text, out decimal max))
                criteria.MaxPrice = max;

            if (criteria.MinPrice.HasValue && criteria.MaxPrice.HasValue && criteria.MinPrice == criteria.MaxPrice &&
                criteria.NameTerm == null && !criteria.CategoryId.HasValue)
            {
                criteria.ExactPrice = criteria.MinPrice;
            }

            OperationResult<List<Medicine>> result = _searchService.Search(criteria);
            double elapsedMs = (DateTime.UtcNow - start).TotalMilliseconds;

            string algorithmUsed = criteria.ExactPrice.HasValue ? "binary search on price" : "linear scan + filters";
            BindResults(result.IsSuccess ? result.Data : new List<Medicine>(), $"{algorithmUsed}, {elapsedMs:F2} ms");
        }

        private void ClearFilters()
        {
            txtSearch.Text = "";
            cboCategory.SelectedIndex = 0;
            txtMinPrice.Text = "";
            txtMaxPrice.Text = "";
            BindResults(_allMedicines, "linear scan (no filters applied)");
        }

        private void BindResults(List<Medicine> medicines, string algorithmCaption)
        {
            _currentResults = medicines;

            var displayRows = medicines.Select(m => new
            {
                m.Id,
                Name = m.Name,
                Category = _categories.FirstOrDefault(c => c.Id == m.CategoryId)?.Name ?? "-",
                Price = m.UnitPrice.ToString("C2"),
                Promotion = m.DiscountPercent > 0 ? $"{m.DiscountPercent:0}% off" : "",
                Rx = m.RequiresPrescription ? "Rx required" : "",
                Stock = m.StockQuantity > 0 ? m.StockQuantity.ToString() : "Out of stock"
            }).ToList();

            dgvResults.DataSource = displayRows;
            if (dgvResults.Columns.Contains("Id"))
                dgvResults.Columns["Id"].Visible = false;

            lblResultCaption.Text = $"{medicines.Count} result(s) — {algorithmCaption}";
            UpdateAddToCartState();
        }

        private Medicine GetSelectedMedicine()
        {
            if (dgvResults.SelectedRows.Count == 0) return null;
            int rowIndex = dgvResults.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _currentResults.Count) return null;
            return _currentResults[rowIndex];
        }

        private void UpdateAddToCartState()
        {
            Medicine selected = GetSelectedMedicine();
            btnAddToCart.Enabled = selected != null && selected.StockQuantity > 0;
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            Medicine selected = GetSelectedMedicine();
            if (selected == null) return;

            int quantity = (int)nudQuantity.Value;
            _cartService.AddItem(selected, quantity);
            ToastNotifier.Show(FindForm(), $"Added {quantity} × {selected.Name} to cart.");
        }
    }
}
