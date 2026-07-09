using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    public class MedicineForm : Form
    {
        private readonly IMedicineService _medicineService;
        private readonly IMedicineCategoryService _categoryService;

        private ModernTextBox txtSearch;
        private RoundedButton btnSearch;
        private RoundedButton btnAddNew;
        private ComboBox cboCategory;
        private ModernTextBox txtName;
        private ModernTextBox txtBrand;
        private ComboBox cboDosageForm;
        private ModernTextBox txtStrength;
        private ModernTextBox txtUnit;
        private ModernTextBox txtStockQuantity;
        private ModernTextBox txtReorderLevel;
        private ModernTextBox txtUnitPrice;
        private ModernTextBox txtDiscountPercent;
        private ModernTextBox txtPromotionLabel;
        private CheckBox chkRequiresPrescription;
        private DateTimePicker dtpExpiry;
        private RoundedButton btnAdd;
        private RoundedButton btnUpdate;
        private RoundedButton btnDelete;
        private RoundedButton btnClearForm;
        private DataGridView dgvMedicines;
        private Label lblLowStockAlert;
        private Label lblNearExpiryAlert;

        private List<MedicineCategory> _categories;
        private List<Medicine> _currentMedicines;
        private int _selectedMedicineId;
        private string _selectedMedicineDescription;

        public MedicineForm(IMedicineService medicineService, IMedicineCategoryService categoryService)
        {
            _medicineService = medicineService;
            _categoryService = categoryService;
            InitializeComponents();
            LoadCategories();
            LoadMedicines();
        }

        private void InitializeComponents()
        {
            Text = "Medicines";
            BackColor = AppTheme.Background;
            AutoScroll = true;

            Label title = new Label { AutoSize = true, Font = AppTheme.PageTitle, ForeColor = AppTheme.TextPrimary, Location = new Point(0, 0), Text = "Medicines" };
            Controls.Add(title);

            txtSearch = new ModernTextBox { Location = new Point(0, 48), Width = 280, PlaceholderText = "Search medicines", LeadingIcon = IconFactory.Search };
            Controls.Add(txtSearch);

            btnSearch = new RoundedButton { Variant = ButtonVariant.Outline, Text = "Search", Location = new Point(292, 48), Width = 100 };
            btnSearch.Click += BtnSearch_Click;
            Controls.Add(btnSearch);

            btnAddNew = new RoundedButton { Variant = ButtonVariant.Primary, IconGlyph = IconFactory.Add, Text = "New Medicine", Location = new Point(700, 48), Width = 160 };
            btnAddNew.Click += (s, e) => ClearFields();
            Controls.Add(btnAddNew);

            lblLowStockAlert = new Label { AutoSize = true, Font = AppTheme.CaptionBold, ForeColor = AppTheme.Danger, Location = new Point(0, 96), Text = "" };
            Controls.Add(lblLowStockAlert);
            lblNearExpiryAlert = new Label { AutoSize = true, Font = AppTheme.CaptionBold, ForeColor = AppTheme.Warning, Location = new Point(320, 96), Text = "" };
            Controls.Add(lblNearExpiryAlert);

            dgvMedicines = new DataGridView { Location = new Point(0, 128), Size = new Size(900, 260), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            DataGridViewStyler.Apply(dgvMedicines);
            dgvMedicines.SelectionChanged += DgvMedicines_SelectionChanged;
            Controls.Add(dgvMedicines);

            CardPanel formCard = new CardPanel { Location = new Point(0, 400), Size = new Size(900, 300) };
            Controls.Add(formCard);

            int labelX = 20;
            int fieldX = 130;
            int fieldWidth = 180;
            int rowH = 36;
            int colGap = 260;

            Label lblCategory = new Label { AutoSize = true, Font = AppTheme.Body, Location = new Point(labelX, 24), Text = "Category:" };
            formCard.Controls.Add(lblCategory);
            cboCategory = new ComboBox { Font = AppTheme.Body, Location = new Point(fieldX, 20), Width = fieldWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            formCard.Controls.Add(cboCategory);

            txtName = AddFormField(formCard, "Name:", labelX + colGap, fieldX + colGap, 20, fieldWidth);

            int row2 = 20 + rowH;
            txtBrand = AddFormField(formCard, "Brand:", labelX, fieldX, row2, fieldWidth);

            Label lblDosageForm = new Label { AutoSize = true, Font = AppTheme.Body, Location = new Point(labelX + colGap, row2 + 4), Text = "Dosage Form:" };
            formCard.Controls.Add(lblDosageForm);
            cboDosageForm = new ComboBox { Font = AppTheme.Body, Location = new Point(fieldX + colGap, row2), Width = fieldWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            formCard.Controls.Add(cboDosageForm);

            int row3 = row2 + rowH;
            txtStrength = AddFormField(formCard, "Strength:", labelX, fieldX, row3, fieldWidth);
            txtUnit = AddFormField(formCard, "Unit:", labelX + colGap, fieldX + colGap, row3, fieldWidth);

            int row4 = row3 + rowH;
            txtStockQuantity = AddFormField(formCard, "Stock Qty:", labelX, fieldX, row4, fieldWidth);
            txtReorderLevel = AddFormField(formCard, "Reorder Level:", labelX + colGap, fieldX + colGap, row4, fieldWidth);

            int row5 = row4 + rowH;
            txtUnitPrice = AddFormField(formCard, "Unit Price:", labelX, fieldX, row5, fieldWidth);

            Label lblExpiry = new Label { AutoSize = true, Font = AppTheme.Body, Location = new Point(labelX + colGap, row5 + 4), Text = "Expiry Date:" };
            formCard.Controls.Add(lblExpiry);
            dtpExpiry = new DateTimePicker { Font = AppTheme.Body, Location = new Point(fieldX + colGap, row5), Width = fieldWidth, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
            formCard.Controls.Add(dtpExpiry);

            int row6 = row5 + rowH;
            txtDiscountPercent = AddFormField(formCard, "Discount %:", labelX, fieldX, row6, fieldWidth);
            txtPromotionLabel = AddFormField(formCard, "Promotion:", labelX + colGap, fieldX + colGap, row6, fieldWidth);

            int row7 = row6 + rowH;
            chkRequiresPrescription = new CheckBox { Font = AppTheme.Body, Location = new Point(labelX, row7), AutoSize = true, Text = "Requires prescription" };
            formCard.Controls.Add(chkRequiresPrescription);

            int buttonsY = row7 + 40;
            btnAdd = new RoundedButton { Variant = ButtonVariant.Primary, Text = "Add", Location = new Point(labelX, buttonsY), Width = 100 };
            btnAdd.Click += BtnAdd_Click;
            formCard.Controls.Add(btnAdd);

            btnUpdate = new RoundedButton { Variant = ButtonVariant.Secondary, Text = "Update", Location = new Point(labelX + 108, buttonsY), Width = 100 };
            btnUpdate.Click += BtnUpdate_Click;
            formCard.Controls.Add(btnUpdate);

            btnDelete = new RoundedButton { Variant = ButtonVariant.Danger, Text = "Delete", Location = new Point(labelX + 216, buttonsY), Width = 100 };
            btnDelete.Click += BtnDelete_Click;
            formCard.Controls.Add(btnDelete);

            btnClearForm = new RoundedButton { Variant = ButtonVariant.Ghost, Text = "Clear", Location = new Point(labelX + 324, buttonsY), Width = 100 };
            btnClearForm.Click += (s, e) => ClearFields();
            formCard.Controls.Add(btnClearForm);
        }

        private ModernTextBox AddFormField(Control parent, string labelText, int labelX, int fieldX, int y, int width)
        {
            Label label = new Label { AutoSize = true, Font = AppTheme.Body, Location = new Point(labelX, y + 10), Text = labelText };
            parent.Controls.Add(label);
            ModernTextBox field = new ModernTextBox { Location = new Point(fieldX, y), Width = width };
            parent.Controls.Add(field);
            return field;
        }

        private void LoadCategories()
        {
            try
            {
                OperationResult<List<MedicineCategory>> result = _categoryService.GetAllCategories();
                if (result.IsSuccess)
                {
                    _categories = result.Data;
                    cboCategory.Items.Clear();
                    cboCategory.Items.Add("-- Select Category --");
                    foreach (MedicineCategory cat in _categories)
                        cboCategory.Items.Add(cat.Name);
                    cboCategory.SelectedIndex = 0;
                }
            }
            catch
            {
                _categories = new List<MedicineCategory>();
            }

            cboDosageForm.Items.Clear();
            foreach (DosageForm form in Enum.GetValues(typeof(DosageForm)))
                cboDosageForm.Items.Add(form);
        }

        private void LoadMedicines()
        {
            try
            {
                OperationResult<List<Medicine>> result = _medicineService.GetAllMedicines();
                if (result.IsSuccess)
                {
                    _currentMedicines = result.Data;
                    BindMedicines(_currentMedicines);
                }
            }
            catch
            {
                _currentMedicines = new List<Medicine>();
            }

            UpdateAlerts();
        }

        private void BindMedicines(List<Medicine> medicines)
        {
            var displayData = medicines.ConvertAll(m => new
            {
                m.Id,
                Category = GetCategoryName(m.CategoryId),
                m.Name,
                m.Brand,
                DosageForm = m.DosageForm.ToString(),
                Stock = m.StockQuantity,
                Price = m.UnitPrice.ToString("C2"),
                Discount = m.DiscountPercent > 0 ? $"{m.DiscountPercent:0}%" : "",
                Rx = m.RequiresPrescription ? "Yes" : ""
            });

            dgvMedicines.DataSource = null;
            dgvMedicines.DataSource = displayData;

            if (dgvMedicines.Columns.Contains("Id"))
                dgvMedicines.Columns["Id"].Visible = false;
        }

        private void UpdateAlerts()
        {
            try
            {
                OperationResult<List<Medicine>> lowStock = _medicineService.GetLowStockMedicines();
                if (lowStock.IsSuccess)
                {
                    int count = lowStock.Data.Count;
                    lblLowStockAlert.Text = count > 0 ? $"⚠ Low Stock: {count} medicine(s)" : "";
                }

                OperationResult<List<Medicine>> nearExpiry = _medicineService.GetNearExpiryMedicines();
                if (nearExpiry.IsSuccess)
                {
                    int count = nearExpiry.Data.Count;
                    lblNearExpiryAlert.Text = count > 0 ? $"⚠ Near Expiry: {count} medicine(s)" : "";
                }
            }
            catch
            {
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadMedicines();
                return;
            }

            OperationResult<List<Medicine>> result = _medicineService.SearchMedicines(keyword);
            if (result.IsSuccess)
            {
                _currentMedicines = result.Data;
                BindMedicines(_currentMedicines);
            }
        }

        private void DgvMedicines_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMedicines.SelectedRows.Count > 0)
            {
                int rowIndex = dgvMedicines.SelectedRows[0].Index;
                if (rowIndex >= 0 && rowIndex < _currentMedicines.Count)
                {
                    Medicine medicine = _currentMedicines[rowIndex];
                    _selectedMedicineId = medicine.Id;
                    _selectedMedicineDescription = medicine.Description;
                    PopulateFields(medicine);
                }
            }
        }

        private void PopulateFields(Medicine medicine)
        {
            if (medicine.CategoryId > 0)
            {
                int catIndex = _categories.FindIndex(c => c.Id == medicine.CategoryId);
                cboCategory.SelectedIndex = catIndex >= 0 ? catIndex + 1 : 0;
            }
            else
            {
                cboCategory.SelectedIndex = 0;
            }

            txtName.Text = medicine.Name;
            txtBrand.Text = medicine.Brand ?? "";

            for (int i = 0; i < cboDosageForm.Items.Count; i++)
            {
                if ((DosageForm)cboDosageForm.Items[i] == medicine.DosageForm)
                {
                    cboDosageForm.SelectedIndex = i;
                    break;
                }
            }

            txtStrength.Text = medicine.Strength ?? "";
            txtUnit.Text = medicine.Unit ?? "";
            txtStockQuantity.Text = medicine.StockQuantity.ToString();
            txtReorderLevel.Text = medicine.ReorderLevel.ToString();
            txtUnitPrice.Text = medicine.UnitPrice.ToString("F2");
            txtDiscountPercent.Text = medicine.DiscountPercent.ToString("F0");
            txtPromotionLabel.Text = medicine.PromotionLabel ?? "";
            chkRequiresPrescription.Checked = medicine.RequiresPrescription;

            if (medicine.ExpiryDate.HasValue)
            {
                dtpExpiry.Checked = true;
                dtpExpiry.Value = medicine.ExpiryDate.Value;
            }
            else
            {
                dtpExpiry.Checked = false;
            }

        }

        private void ClearFields()
        {
            _selectedMedicineId = 0;
            _selectedMedicineDescription = null;
            cboCategory.SelectedIndex = 0;
            txtName.Text = "";
            txtBrand.Text = "";
            cboDosageForm.SelectedIndex = -1;
            txtStrength.Text = "";
            txtUnit.Text = "";
            txtStockQuantity.Text = "";
            txtReorderLevel.Text = "";
            txtUnitPrice.Text = "";
            txtDiscountPercent.Text = "";
            txtPromotionLabel.Text = "";
            chkRequiresPrescription.Checked = false;
            dtpExpiry.Checked = false;
            dgvMedicines.ClearSelection();
        }

        private Medicine ReadMedicineFromFields()
        {
            int categoryId = 0;
            if (cboCategory.SelectedIndex > 0 && _categories != null && cboCategory.SelectedIndex - 1 < _categories.Count)
                categoryId = _categories[cboCategory.SelectedIndex - 1].Id;

            DosageForm dosageForm = DosageForm.Tablet;
            if (cboDosageForm.SelectedIndex >= 0)
                dosageForm = (DosageForm)cboDosageForm.Items[cboDosageForm.SelectedIndex];

            int.TryParse(txtStockQuantity.Text.Trim(), out int stockQty);
            int.TryParse(txtReorderLevel.Text.Trim(), out int reorderLevel);
            decimal.TryParse(txtUnitPrice.Text.Trim(), out decimal unitPrice);
            decimal.TryParse(txtDiscountPercent.Text.Trim(), out decimal discountPercent);

            DateTime? expiryDate = dtpExpiry.Checked ? dtpExpiry.Value : (DateTime?)null;

            return new Medicine
            {
                CategoryId = categoryId,
                Name = txtName.Text.Trim(),
                Brand = txtBrand.Text.Trim(),
                DosageForm = dosageForm,
                Strength = txtStrength.Text.Trim(),
                Unit = txtUnit.Text.Trim(),
                StockQuantity = stockQty,
                ReorderLevel = reorderLevel,
                UnitPrice = unitPrice,
                DiscountPercent = discountPercent,
                PromotionLabel = string.IsNullOrWhiteSpace(txtPromotionLabel.Text) ? null : txtPromotionLabel.Text.Trim(),
                RequiresPrescription = chkRequiresPrescription.Checked,
                ExpiryDate = expiryDate,
                Description = _selectedMedicineDescription
            };
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Medicine medicine = ReadMedicineFromFields();
            OperationResult<int> result = _medicineService.AddMedicine(medicine);

            if (result.IsSuccess)
            {
                ClearFields();
                LoadMedicines();
                ToastNotifier.Show(FindForm(), "Medicine added successfully.");
            }
            else
            {
                ToastNotifier.Show(FindForm(), result.Message, ToastKind.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedMedicineId <= 0)
            {
                ToastNotifier.Show(FindForm(), "Select a medicine to update.", ToastKind.Warning);
                return;
            }

            Medicine medicine = ReadMedicineFromFields();
            medicine.Id = _selectedMedicineId;

            OperationResult result = _medicineService.UpdateMedicine(medicine);

            if (result.IsSuccess)
            {
                LoadMedicines();
                ToastNotifier.Show(FindForm(), "Medicine updated successfully.");
            }
            else
            {
                ToastNotifier.Show(FindForm(), result.Message, ToastKind.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedMedicineId <= 0)
            {
                ToastNotifier.Show(FindForm(), "Select a medicine to delete.", ToastKind.Warning);
                return;
            }

            Medicine selected = _currentMedicines.Find(m => m.Id == _selectedMedicineId);
            string medicineName = selected?.Name ?? "this medicine";
            int id = _selectedMedicineId;

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to delete '{medicineName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            OperationResult result = _medicineService.DeleteMedicine(id);

            if (result.IsSuccess)
            {
                ClearFields();
                LoadMedicines();
                ToastNotifier.Show(FindForm(), "Medicine deleted successfully.");
            }
            else
            {
                ToastNotifier.Show(FindForm(), result.Message, ToastKind.Error);
            }
        }

        private string GetCategoryName(int categoryId)
        {
            MedicineCategory cat = _categories?.Find(c => c.Id == categoryId);
            return cat?.Name ?? "Unknown";
        }
    }
}
