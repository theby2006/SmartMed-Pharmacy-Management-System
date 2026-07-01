using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.UI.Forms
{
    public class SupplierForm : Form
    {
        private readonly ISupplierService _supplierService;

        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnClearSearch;
        private ComboBox cboStatusFilter;
        private TextBox txtSupplierCode;
        private TextBox txtSupplierName;
        private TextBox txtCompanyName;
        private TextBox txtContactPerson;
        private TextBox txtPhoneNumber;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private TextBox txtCity;
        private TextBox txtCountry;
        private TextBox txtPostalCode;
        private TextBox txtTaxNumber;
        private TextBox txtNotes;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRefresh;
        private DataGridView dgvSuppliers;
        private Label lblStatus;

        private List<Supplier> _currentSuppliers;
        private int _selectedSupplierId;

        public SupplierForm(ISupplierService supplierService)
        {
            _supplierService = supplierService;
            InitializeComponents();
            LoadSuppliers();
        }

        private void InitializeComponents()
        {
            Text = "SmartMed - Suppliers";
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
            btnClearSearch.Click += (s, e) => { txtSearch.Text = ""; LoadSuppliers(); };

            int row1 = 56;
            Label lblSupplierCode = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row1),
                Text = "Supplier Code:"
            };

            txtSupplierCode = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row1 - 3),
                Width = fieldWidth
            };

            Label lblSupplierName = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row1),
                Text = "Supplier Name:"
            };

            txtSupplierName = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row1 - 3),
                Width = fieldWidth
            };

            int row2 = row1 + rowH;
            Label lblCompanyName = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row2),
                Text = "Company Name:"
            };

            txtCompanyName = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row2 - 3),
                Width = fieldWidth
            };

            Label lblContactPerson = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row2),
                Text = "Contact Person:"
            };

            txtContactPerson = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row2 - 3),
                Width = fieldWidth
            };

            int row3 = row2 + rowH;
            Label lblPhoneNumber = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row3),
                Text = "Phone Number:"
            };

            txtPhoneNumber = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row3 - 3),
                Width = fieldWidth
            };

            Label lblEmail = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row3),
                Text = "Email:"
            };

            txtEmail = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row3 - 3),
                Width = fieldWidth
            };

            int row4 = row3 + rowH;
            Label lblAddress = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row4),
                Text = "Address:"
            };

            txtAddress = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row4 - 3),
                Width = fieldWidth
            };

            Label lblCity = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row4),
                Text = "City:"
            };

            txtCity = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row4 - 3),
                Width = fieldWidth
            };

            int row5 = row4 + rowH;
            Label lblCountry = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row5),
                Text = "Country:"
            };

            txtCountry = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row5 - 3),
                Width = fieldWidth
            };

            Label lblPostalCode = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row5),
                Text = "Postal Code:"
            };

            txtPostalCode = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row5 - 3),
                Width = fieldWidth
            };

            int row6 = row5 + rowH;
            Label lblTaxNumber = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row6),
                Text = "Tax Number:"
            };

            txtTaxNumber = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row6 - 3),
                Width = fieldWidth
            };

            Label lblNotes = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row6),
                Text = "Notes:"
            };

            txtNotes = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row6 - 3),
                Width = fieldWidth,
                Height = 50,
                Multiline = true
            };

            int buttonsY = row6 + 70;
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
            btnRefresh.Click += (s, e) => { txtSearch.Text = ""; LoadSuppliers(); };

            int filterY = buttonsY + 40;
            Label lblFilter = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, filterY),
                Text = "Status:"
            };

            cboStatusFilter = new ComboBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, filterY - 3),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatusFilter.Items.AddRange(new object[] { "All Suppliers", "Active Only", "Inactive Only" });
            cboStatusFilter.SelectedIndex = 1;
            cboStatusFilter.SelectedIndexChanged += CboStatusFilter_SelectedIndexChanged;

            int gridY = filterY + 30;
            dgvSuppliers = new DataGridView
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
            dgvSuppliers.SelectionChanged += DgvSuppliers_SelectionChanged;

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
            Controls.Add(lblSupplierCode);
            Controls.Add(txtSupplierCode);
            Controls.Add(lblSupplierName);
            Controls.Add(txtSupplierName);
            Controls.Add(lblCompanyName);
            Controls.Add(txtCompanyName);
            Controls.Add(lblContactPerson);
            Controls.Add(txtContactPerson);
            Controls.Add(lblPhoneNumber);
            Controls.Add(txtPhoneNumber);
            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(lblAddress);
            Controls.Add(txtAddress);
            Controls.Add(lblCity);
            Controls.Add(txtCity);
            Controls.Add(lblCountry);
            Controls.Add(txtCountry);
            Controls.Add(lblPostalCode);
            Controls.Add(txtPostalCode);
            Controls.Add(lblTaxNumber);
            Controls.Add(txtTaxNumber);
            Controls.Add(lblNotes);
            Controls.Add(txtNotes);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(btnRefresh);
            Controls.Add(lblFilter);
            Controls.Add(cboStatusFilter);
            Controls.Add(dgvSuppliers);
            Controls.Add(lblStatus);
        }

        private void LoadSuppliers()
        {
            try
            {
                OperationResult<List<Supplier>> result = _supplierService.GetAllSuppliers();
                if (result.IsSuccess)
                {
                    _currentSuppliers = result.Data;
                    BindSuppliers(_currentSuppliers);
                    SetStatus($"Loaded {_currentSuppliers.Count} supplier(s).", Color.Green);
                }
                else
                {
                    SetStatus(result.Message, Color.Red);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error loading suppliers: {ex.Message}", Color.Red);
            }

            _selectedSupplierId = 0;
        }

        private void BindSuppliers(List<Supplier> suppliers)
        {
            dgvSuppliers.DataSource = null;
            dgvSuppliers.DataSource = suppliers;

            if (dgvSuppliers.Columns.Contains("Id"))
                dgvSuppliers.Columns["Id"].Width = 35;
            if (dgvSuppliers.Columns.Contains("SupplierCode"))
                dgvSuppliers.Columns["SupplierCode"].Width = 90;
            if (dgvSuppliers.Columns.Contains("SupplierName"))
                dgvSuppliers.Columns["SupplierName"].Width = 140;
            if (dgvSuppliers.Columns.Contains("CompanyName"))
                dgvSuppliers.Columns["CompanyName"].Width = 120;
            if (dgvSuppliers.Columns.Contains("ContactPerson"))
                dgvSuppliers.Columns["ContactPerson"].Width = 100;
            if (dgvSuppliers.Columns.Contains("PhoneNumber"))
                dgvSuppliers.Columns["PhoneNumber"].Width = 90;
            if (dgvSuppliers.Columns.Contains("Email"))
                dgvSuppliers.Columns["Email"].Visible = false;
            if (dgvSuppliers.Columns.Contains("Address"))
                dgvSuppliers.Columns["Address"].Visible = false;
            if (dgvSuppliers.Columns.Contains("City"))
                dgvSuppliers.Columns["City"].Width = 80;
            if (dgvSuppliers.Columns.Contains("Country"))
                dgvSuppliers.Columns["Country"].Visible = false;
            if (dgvSuppliers.Columns.Contains("PostalCode"))
                dgvSuppliers.Columns["PostalCode"].Visible = false;
            if (dgvSuppliers.Columns.Contains("TaxNumber"))
                dgvSuppliers.Columns["TaxNumber"].Visible = false;
            if (dgvSuppliers.Columns.Contains("Notes"))
                dgvSuppliers.Columns["Notes"].Visible = false;
            if (dgvSuppliers.Columns.Contains("IsActive"))
                dgvSuppliers.Columns["IsActive"].Visible = false;
            if (dgvSuppliers.Columns.Contains("CreatedDate"))
                dgvSuppliers.Columns["CreatedDate"].Visible = false;
            if (dgvSuppliers.Columns.Contains("UpdatedDate"))
                dgvSuppliers.Columns["UpdatedDate"].Visible = false;
        }

        private void ApplyStatusFilter()
        {
            string filter = cboStatusFilter.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(filter) || filter == "All Suppliers")
            {
                BindSuppliers(_currentSuppliers);
            }
            else if (filter == "Active Only")
            {
                BindSuppliers(_currentSuppliers.FindAll(s => s.IsActive));
            }
            else if (filter == "Inactive Only")
            {
                BindSuppliers(_currentSuppliers.FindAll(s => !s.IsActive));
            }
        }

        private void CboStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyStatusFilter();
        }

        private void DgvSuppliers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count > 0)
            {
                Supplier supplier = dgvSuppliers.SelectedRows[0].DataBoundItem as Supplier;
                if (supplier != null)
                {
                    _selectedSupplierId = supplier.Id;
                    PopulateFields(supplier);
                }
            }
        }

        private void PopulateFields(Supplier supplier)
        {
            txtSupplierCode.Text = supplier.SupplierCode;
            txtSupplierName.Text = supplier.SupplierName;
            txtCompanyName.Text = supplier.CompanyName ?? "";
            txtContactPerson.Text = supplier.ContactPerson ?? "";
            txtPhoneNumber.Text = supplier.PhoneNumber ?? "";
            txtEmail.Text = supplier.Email ?? "";
            txtAddress.Text = supplier.Address ?? "";
            txtCity.Text = supplier.City ?? "";
            txtCountry.Text = supplier.Country ?? "";
            txtPostalCode.Text = supplier.PostalCode ?? "";
            txtTaxNumber.Text = supplier.TaxNumber ?? "";
            txtNotes.Text = supplier.Notes ?? "";
        }

        private void ClearFields()
        {
            txtSupplierCode.Text = "";
            txtSupplierName.Text = "";
            txtCompanyName.Text = "";
            txtContactPerson.Text = "";
            txtPhoneNumber.Text = "";
            txtEmail.Text = "";
            txtAddress.Text = "";
            txtCity.Text = "";
            txtCountry.Text = "";
            txtPostalCode.Text = "";
            txtTaxNumber.Text = "";
            txtNotes.Text = "";
        }

        private Supplier ReadSupplierFromFields()
        {
            return new Supplier
            {
                SupplierCode = txtSupplierCode.Text.Trim(),
                SupplierName = txtSupplierName.Text.Trim(),
                CompanyName = txtCompanyName.Text.Trim(),
                ContactPerson = txtContactPerson.Text.Trim(),
                PhoneNumber = txtPhoneNumber.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                City = txtCity.Text.Trim(),
                Country = txtCountry.Text.Trim(),
                PostalCode = txtPostalCode.Text.Trim(),
                TaxNumber = txtTaxNumber.Text.Trim(),
                Notes = txtNotes.Text.Trim()
            };
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Supplier supplier = ReadSupplierFromFields();
            OperationResult<int> result = _supplierService.AddSupplier(supplier);

            if (result.IsSuccess)
            {
                ClearFields();
                LoadSuppliers();
                SetStatus("Supplier added successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedSupplierId <= 0)
            {
                SetStatus("Please select a supplier to update.", Color.Red);
                return;
            }

            Supplier supplier = ReadSupplierFromFields();
            supplier.Id = _selectedSupplierId;

            OperationResult result = _supplierService.UpdateSupplier(supplier);

            if (result.IsSuccess)
            {
                LoadSuppliers();
                SetStatus("Supplier updated successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedSupplierId <= 0)
            {
                SetStatus("Please select a supplier to delete.", Color.Red);
                return;
            }

            Supplier selected = _currentSuppliers?.Find(s => s.Id == _selectedSupplierId);
            string supplierName = selected?.SupplierName ?? "this supplier";
            int id = _selectedSupplierId;

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to delete '{supplierName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            OperationResult result = _supplierService.DeleteSupplier(id);

            if (result.IsSuccess)
            {
                ClearFields();
                LoadSuppliers();
                SetStatus("Supplier deleted successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadSuppliers();
                return;
            }

            try
            {
                OperationResult<List<Supplier>> result = _supplierService.SearchSuppliers(keyword);
                if (result.IsSuccess)
                {
                    _currentSuppliers = result.Data;
                    BindSuppliers(_currentSuppliers);
                    SetStatus($"Found {_currentSuppliers.Count} supplier(s).", Color.Green);
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

        private void SetStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
