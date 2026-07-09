using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    public class SupplierForm : Form
    {
        private readonly ISupplierService _supplierService;

        private ModernTextBox txtSearch;
        private RoundedButton btnSearch;
        private RoundedButton btnAddNew;
        private ComboBox cboStatusFilter;
        private ModernTextBox txtSupplierCode;
        private ModernTextBox txtSupplierName;
        private ModernTextBox txtCompanyName;
        private ModernTextBox txtContactPerson;
        private ModernTextBox txtPhoneNumber;
        private ModernTextBox txtEmail;
        private ModernTextBox txtAddress;
        private ModernTextBox txtCity;
        private ModernTextBox txtCountry;
        private ModernTextBox txtPostalCode;
        private ModernTextBox txtTaxNumber;
        private ModernTextBox txtNotes;
        private RoundedButton btnAdd;
        private RoundedButton btnUpdate;
        private RoundedButton btnDelete;
        private RoundedButton btnClearForm;
        private DataGridView dgvSuppliers;

        private List<Supplier> _currentSuppliers;
        private List<Supplier> _displayedSuppliers = new List<Supplier>();
        private int _selectedSupplierId;

        public SupplierForm(ISupplierService supplierService)
        {
            _supplierService = supplierService;
            InitializeComponents();
            LoadSuppliers();
        }

        private void InitializeComponents()
        {
            Text = "Suppliers";
            BackColor = AppTheme.Background;
            AutoScroll = true;

            Label title = new Label { AutoSize = true, Font = AppTheme.PageTitle, ForeColor = AppTheme.TextPrimary, Location = new Point(0, 0), Text = "Suppliers" };
            Controls.Add(title);

            txtSearch = new ModernTextBox { Location = new Point(0, 48), Width = 260, PlaceholderText = "Search suppliers", LeadingIcon = IconFactory.Search };
            Controls.Add(txtSearch);

            btnSearch = new RoundedButton { Variant = ButtonVariant.Outline, Text = "Search", Location = new Point(272, 48), Width = 100 };
            btnSearch.Click += BtnSearch_Click;
            Controls.Add(btnSearch);

            cboStatusFilter = new ComboBox { Font = AppTheme.Body, Location = new Point(384, 58), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatusFilter.Items.AddRange(new object[] { "All Suppliers", "Active Only", "Inactive Only" });
            cboStatusFilter.SelectedIndex = 1;
            cboStatusFilter.SelectedIndexChanged += CboStatusFilter_SelectedIndexChanged;
            Controls.Add(cboStatusFilter);

            btnAddNew = new RoundedButton { Variant = ButtonVariant.Primary, IconGlyph = IconFactory.Add, Text = "New Supplier", Location = new Point(700, 48), Width = 160 };
            btnAddNew.Click += (s, e) => ClearFields();
            Controls.Add(btnAddNew);

            dgvSuppliers = new DataGridView { Location = new Point(0, 100), Size = new Size(900, 260), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            DataGridViewStyler.Apply(dgvSuppliers);
            dgvSuppliers.SelectionChanged += DgvSuppliers_SelectionChanged;
            Controls.Add(dgvSuppliers);

            CardPanel formCard = new CardPanel { Location = new Point(0, 372), Size = new Size(900, 300) };
            Controls.Add(formCard);

            int labelX = 20;
            int fieldX = 140;
            int fieldWidth = 180;
            int rowH = 36;
            int colGap = 260;

            txtSupplierCode = AddFormField(formCard, "Code:", labelX, fieldX, 20, fieldWidth);
            txtSupplierName = AddFormField(formCard, "Name:", labelX + colGap, fieldX + colGap, 20, fieldWidth);

            int row2 = 20 + rowH;
            txtCompanyName = AddFormField(formCard, "Company:", labelX, fieldX, row2, fieldWidth);
            txtContactPerson = AddFormField(formCard, "Contact:", labelX + colGap, fieldX + colGap, row2, fieldWidth);

            int row3 = row2 + rowH;
            txtPhoneNumber = AddFormField(formCard, "Phone:", labelX, fieldX, row3, fieldWidth);
            txtEmail = AddFormField(formCard, "Email:", labelX + colGap, fieldX + colGap, row3, fieldWidth);

            int row4 = row3 + rowH;
            txtAddress = AddFormField(formCard, "Address:", labelX, fieldX, row4, fieldWidth);
            txtCity = AddFormField(formCard, "City:", labelX + colGap, fieldX + colGap, row4, fieldWidth);

            int row5 = row4 + rowH;
            txtCountry = AddFormField(formCard, "Country:", labelX, fieldX, row5, fieldWidth);
            txtPostalCode = AddFormField(formCard, "Postal Code:", labelX + colGap, fieldX + colGap, row5, fieldWidth);

            int row6 = row5 + rowH;
            txtTaxNumber = AddFormField(formCard, "Tax Number:", labelX, fieldX, row6, fieldWidth);
            txtNotes = AddFormField(formCard, "Notes:", labelX + colGap, fieldX + colGap, row6, fieldWidth);

            int buttonsY = row6 + 46;
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

        private void LoadSuppliers()
        {
            OperationResult<List<Supplier>> result = _supplierService.GetAllSuppliers();
            _currentSuppliers = result.IsSuccess ? result.Data : new List<Supplier>();
            ApplyStatusFilter();
            _selectedSupplierId = 0;
        }

        private void BindSuppliers(List<Supplier> suppliers)
        {
            _displayedSuppliers = suppliers;

            var displayData = suppliers.ConvertAll(s => new
            {
                s.SupplierCode,
                s.SupplierName,
                s.CompanyName,
                s.ContactPerson,
                s.PhoneNumber,
                s.City,
                Status = s.IsActive ? "Active" : "Inactive"
            });

            dgvSuppliers.DataSource = null;
            dgvSuppliers.DataSource = displayData;
        }

        private void ApplyStatusFilter()
        {
            string filter = cboStatusFilter.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(filter) || filter == "All Suppliers")
                BindSuppliers(_currentSuppliers);
            else if (filter == "Active Only")
                BindSuppliers(_currentSuppliers.FindAll(s => s.IsActive));
            else if (filter == "Inactive Only")
                BindSuppliers(_currentSuppliers.FindAll(s => !s.IsActive));
        }

        private void CboStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyStatusFilter();
        }

        private void DgvSuppliers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count == 0) return;
            int rowIndex = dgvSuppliers.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _displayedSuppliers.Count) return;

            Supplier supplier = _displayedSuppliers[rowIndex];
            _selectedSupplierId = supplier.Id;
            PopulateFields(supplier);
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
            _selectedSupplierId = 0;
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
            dgvSuppliers.ClearSelection();
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
                ToastNotifier.Show(FindForm(), "Supplier added successfully.");
            }
            else
            {
                ToastNotifier.Show(FindForm(), result.Message, ToastKind.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedSupplierId <= 0)
            {
                ToastNotifier.Show(FindForm(), "Select a supplier to update.", ToastKind.Warning);
                return;
            }

            Supplier supplier = ReadSupplierFromFields();
            supplier.Id = _selectedSupplierId;

            OperationResult result = _supplierService.UpdateSupplier(supplier);

            if (result.IsSuccess)
            {
                LoadSuppliers();
                ToastNotifier.Show(FindForm(), "Supplier updated successfully.");
            }
            else
            {
                ToastNotifier.Show(FindForm(), result.Message, ToastKind.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedSupplierId <= 0)
            {
                ToastNotifier.Show(FindForm(), "Select a supplier to delete.", ToastKind.Warning);
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
                ToastNotifier.Show(FindForm(), "Supplier deleted successfully.");
            }
            else
            {
                ToastNotifier.Show(FindForm(), result.Message, ToastKind.Error);
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

            OperationResult<List<Supplier>> result = _supplierService.SearchSuppliers(keyword);
            if (result.IsSuccess)
            {
                _currentSuppliers = result.Data;
                BindSuppliers(_currentSuppliers);
            }
        }
    }
}
