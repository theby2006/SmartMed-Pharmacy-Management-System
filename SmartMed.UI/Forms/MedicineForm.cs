using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.UI.Forms
{
    public class MedicineForm : Form
    {
        private readonly IMedicineService _medicineService;
        private readonly IMedicineCategoryService _categoryService;

        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnClearSearch;
        private ComboBox cboCategory;
        private TextBox txtName;
        private TextBox txtBrand;
        private ComboBox cboDosageForm;
        private TextBox txtStrength;
        private TextBox txtUnit;
        private TextBox txtStockQuantity;
        private TextBox txtReorderLevel;
        private TextBox txtUnitPrice;
        private DateTimePicker dtpExpiry;
        private TextBox txtDescription;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRefresh;
        private DataGridView dgvMedicines;
        private Label lblLowStockAlert;
        private Label lblNearExpiryAlert;
        private Label lblStatus;

        private List<MedicineCategory> _categories;
        private List<Medicine> _currentMedicines;
        private int _selectedMedicineId;

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
            Text = "SmartMed - Medicines";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(860, 680);
            ShowIcon = false;

            int labelX = 16;
            int fieldX = 110;
            int fieldWidth = 160;
            int rowH = 30;
            int colGap = 220;

            Label lblSearch = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, 16),
                Text = "Search:"
            };

            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, 13),
                Width = 200
            };

            btnSearch = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(320, 12),
                Width = 70,
                Height = 26,
                Text = "Search"
            };
            btnSearch.Click += BtnSearch_Click;

            btnClearSearch = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(396, 12),
                Width = 70,
                Height = 26,
                Text = "Clear"
            };
            btnClearSearch.Click += (s, e) => { txtSearch.Text = ""; LoadMedicines(); };

            int row1 = 56;
            Label lblCategory = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row1),
                Text = "Category:"
            };

            cboCategory = new ComboBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row1 - 3),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblName = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row1),
                Text = "Name:"
            };

            txtName = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row1 - 3),
                Width = fieldWidth
            };

            int row2 = row1 + rowH;
            Label lblBrand = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row2),
                Text = "Brand:"
            };

            txtBrand = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row2 - 3),
                Width = fieldWidth
            };

            Label lblDosageForm = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row2),
                Text = "Dosage Form:"
            };

            cboDosageForm = new ComboBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row2 - 3),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            int row3 = row2 + rowH;
            Label lblStrength = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row3),
                Text = "Strength:"
            };

            txtStrength = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row3 - 3),
                Width = fieldWidth
            };

            Label lblUnit = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row3),
                Text = "Unit:"
            };

            txtUnit = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row3 - 3),
                Width = fieldWidth
            };

            int row4 = row3 + rowH;
            Label lblStockQty = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row4),
                Text = "Stock Qty:"
            };

            txtStockQuantity = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row4 - 3),
                Width = fieldWidth
            };

            Label lblReorder = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row4),
                Text = "Reorder Level:"
            };

            txtReorderLevel = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row4 - 3),
                Width = fieldWidth
            };

            int row5 = row4 + rowH;
            Label lblPrice = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row5),
                Text = "Unit Price:"
            };

            txtUnitPrice = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row5 - 3),
                Width = fieldWidth
            };

            Label lblExpiry = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row5),
                Text = "Expiry Date:"
            };

            dtpExpiry = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row5 - 3),
                Width = fieldWidth,
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };

            int row6 = row5 + rowH;
            Label lblDescription = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row6),
                Text = "Description:"
            };

            txtDescription = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row6 - 3),
                Width = 350,
                Height = 50,
                Multiline = true
            };

            int buttonsY = row6 + 60;
            btnAdd = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, buttonsY),
                Width = 80,
                Height = 28,
                Text = "Add"
            };
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(104, buttonsY),
                Width = 80,
                Height = 28,
                Text = "Update"
            };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(192, buttonsY),
                Width = 80,
                Height = 28,
                Text = "Delete"
            };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(280, buttonsY),
                Width = 80,
                Height = 28,
                Text = "Refresh"
            };
            btnRefresh.Click += (s, e) => { txtSearch.Text = ""; LoadMedicines(); };

            int alertsY = buttonsY + 40;
            lblLowStockAlert = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.Red,
                Location = new Point(labelX, alertsY),
                Text = ""
            };

            lblNearExpiryAlert = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                Location = new Point(300, alertsY),
                Text = ""
            };

            int gridY = alertsY + 24;
            dgvMedicines = new DataGridView
            {
                Location = new Point(labelX, gridY),
                Width = 820,
                Height = 260,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvMedicines.SelectionChanged += DgvMedicines_SelectionChanged;

            lblStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Green,
                Location = new Point(labelX, gridY + 270),
                Width = 800,
                Text = "Ready"
            };

            Controls.Add(lblSearch);
            Controls.Add(txtSearch);
            Controls.Add(btnSearch);
            Controls.Add(btnClearSearch);
            Controls.Add(lblCategory);
            Controls.Add(cboCategory);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblBrand);
            Controls.Add(txtBrand);
            Controls.Add(lblDosageForm);
            Controls.Add(cboDosageForm);
            Controls.Add(lblStrength);
            Controls.Add(txtStrength);
            Controls.Add(lblUnit);
            Controls.Add(txtUnit);
            Controls.Add(lblStockQty);
            Controls.Add(txtStockQuantity);
            Controls.Add(lblReorder);
            Controls.Add(txtReorderLevel);
            Controls.Add(lblPrice);
            Controls.Add(txtUnitPrice);
            Controls.Add(lblExpiry);
            Controls.Add(dtpExpiry);
            Controls.Add(lblDescription);
            Controls.Add(txtDescription);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(btnRefresh);
            Controls.Add(lblLowStockAlert);
            Controls.Add(lblNearExpiryAlert);
            Controls.Add(dgvMedicines);
            Controls.Add(lblStatus);
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
                    SetStatus($"Loaded {_currentMedicines.Count} medicines.", Color.Green);
                }
                else
                {
                    SetStatus(result.Message, Color.Red);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error loading medicines: {ex.Message}", Color.Red);
            }

            UpdateAlerts();
        }

        private void BindMedicines(List<Medicine> medicines)
        {
            var displayData = medicines.ConvertAll(m => new MedicineDisplayItem
            {
                Id = m.Id,
                Category = GetCategoryName(m.CategoryId),
                Name = m.Name,
                Brand = m.Brand,
                DosageForm = m.DosageForm.ToString(),
                Strength = m.Strength,
                Unit = m.Unit,
                StockQuantity = m.StockQuantity,
                ReorderLevel = m.ReorderLevel,
                UnitPrice = m.UnitPrice.ToString("C2"),
                ExpiryDate = m.ExpiryDate?.ToString("yyyy-MM-dd") ?? "",
                Description = m.Description
            });

            dgvMedicines.DataSource = null;
            dgvMedicines.DataSource = displayData;

            if (dgvMedicines.Columns.Contains("Id"))
                dgvMedicines.Columns["Id"].Width = 35;
            if (dgvMedicines.Columns.Contains("Category"))
                dgvMedicines.Columns["Category"].Width = 100;
            if (dgvMedicines.Columns.Contains("Name"))
                dgvMedicines.Columns["Name"].Width = 120;
            if (dgvMedicines.Columns.Contains("Brand"))
                dgvMedicines.Columns["Brand"].Width = 90;
            if (dgvMedicines.Columns.Contains("DosageForm"))
                dgvMedicines.Columns["DosageForm"].Width = 80;
            if (dgvMedicines.Columns.Contains("StockQuantity"))
                dgvMedicines.Columns["StockQuantity"].Width = 60;
            if (dgvMedicines.Columns.Contains("UnitPrice"))
                dgvMedicines.Columns["UnitPrice"].Width = 70;
            if (dgvMedicines.Columns.Contains("ExpiryDate"))
                dgvMedicines.Columns["ExpiryDate"].Width = 80;
            if (dgvMedicines.Columns.Contains("Strength"))
                dgvMedicines.Columns["Strength"].Visible = false;
            if (dgvMedicines.Columns.Contains("Unit"))
                dgvMedicines.Columns["Unit"].Visible = false;
            if (dgvMedicines.Columns.Contains("ReorderLevel"))
                dgvMedicines.Columns["ReorderLevel"].Visible = false;
            if (dgvMedicines.Columns.Contains("Description"))
                dgvMedicines.Columns["Description"].Visible = false;
        }

        private void UpdateAlerts()
        {
            try
            {
                OperationResult<List<Medicine>> lowStock = _medicineService.GetLowStockMedicines();
                if (lowStock.IsSuccess)
                {
                    int count = lowStock.Data.Count;
                    lblLowStockAlert.Text = count > 0
                        ? $"Low Stock Alert: {count} medicine(s)"
                        : "All medicines are well-stocked.";
                    lblLowStockAlert.ForeColor = count > 0 ? Color.Red : Color.Green;
                }

                OperationResult<List<Medicine>> nearExpiry = _medicineService.GetNearExpiryMedicines();
                if (nearExpiry.IsSuccess)
                {
                    int count = nearExpiry.Data.Count;
                    lblNearExpiryAlert.Text = count > 0
                        ? $"Near Expiry Alert: {count} medicine(s)"
                        : "No medicines near expiry.";
                    lblNearExpiryAlert.ForeColor = count > 0 ? Color.OrangeRed : Color.Green;
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

            try
            {
                OperationResult<List<Medicine>> result = _medicineService.SearchMedicines(keyword);
                if (result.IsSuccess)
                {
                    _currentMedicines = result.Data;
                    BindMedicines(_currentMedicines);
                    SetStatus($"Found {_currentMedicines.Count} medicine(s).", Color.Green);
                }
                else
                {
                    SetStatus(result.Message, Color.Red);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Search error: {ex.Message}", Color.Red);
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

            if (medicine.ExpiryDate.HasValue)
            {
                dtpExpiry.Checked = true;
                dtpExpiry.Value = medicine.ExpiryDate.Value;
            }
            else
            {
                dtpExpiry.Checked = false;
            }

            txtDescription.Text = medicine.Description ?? "";
        }

        private void ClearFields()
        {
            cboCategory.SelectedIndex = 0;
            txtName.Text = "";
            txtBrand.Text = "";
            cboDosageForm.SelectedIndex = -1;
            txtStrength.Text = "";
            txtUnit.Text = "";
            txtStockQuantity.Text = "";
            txtReorderLevel.Text = "";
            txtUnitPrice.Text = "";
            dtpExpiry.Checked = false;
            txtDescription.Text = "";
        }

        private Medicine ReadMedicineFromFields()
        {
            int categoryId = 0;
            if (cboCategory.SelectedIndex > 0 && _categories != null && cboCategory.SelectedIndex - 1 < _categories.Count)
                categoryId = _categories[cboCategory.SelectedIndex - 1].Id;

            DosageForm dosageForm = DosageForm.Tablet;
            if (cboDosageForm.SelectedIndex >= 0)
                dosageForm = (DosageForm)cboDosageForm.Items[cboDosageForm.SelectedIndex];

            int stockQty = 0;
            int.TryParse(txtStockQuantity.Text.Trim(), out stockQty);

            int reorderLevel = 0;
            int.TryParse(txtReorderLevel.Text.Trim(), out reorderLevel);

            decimal unitPrice = 0;
            decimal.TryParse(txtUnitPrice.Text.Trim(), out unitPrice);

            DateTime? expiryDate = null;
            if (dtpExpiry.Checked)
                expiryDate = dtpExpiry.Value;

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
                ExpiryDate = expiryDate,
                Description = txtDescription.Text.Trim()
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
                SetStatus("Medicine added successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedMedicineId <= 0)
            {
                SetStatus("Please select a medicine to update.", Color.Red);
                return;
            }

            Medicine medicine = ReadMedicineFromFields();
            medicine.Id = _selectedMedicineId;

            OperationResult result = _medicineService.UpdateMedicine(medicine);

            if (result.IsSuccess)
            {
                LoadMedicines();
                SetStatus("Medicine updated successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedMedicineId <= 0)
            {
                SetStatus("Please select a medicine to delete.", Color.Red);
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
                SetStatus("Medicine deleted successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private string GetCategoryName(int categoryId)
        {
            MedicineCategory cat = _categories?.Find(c => c.Id == categoryId);
            return cat?.Name ?? "Unknown";
        }

        private void SetStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }

    internal class MedicineDisplayItem
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string DosageForm { get; set; }
        public string Strength { get; set; }
        public string Unit { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public string UnitPrice { get; set; }
        public string ExpiryDate { get; set; }
        public string Description { get; set; }
    }
}
