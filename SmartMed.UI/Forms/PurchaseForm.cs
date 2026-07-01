using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.UI.Forms
{
    public class PurchaseForm : Form
    {
        private readonly IPurchaseService _purchaseService;
        private readonly IMedicineService _medicineService;
        private readonly ISupplierService _supplierService;

        private DataGridView dgvPurchases;
        private DataGridView dgvItems;
        private ComboBox cboSupplier;
        private TextBox txtPurchaseNumber;
        private TextBox txtInvoiceNumber;
        private DateTimePicker dtpPurchaseDate;
        private TextBox txtRemarks;
        private ComboBox cboMedicine;
        private TextBox txtBatchNumber;
        private DateTimePicker dtpExpiry;
        private NumericUpDown nudQuantity;
        private NumericUpDown nudPurchasePrice;
        private NumericUpDown nudSellingPrice;
        private NumericUpDown nudDiscount;
        private NumericUpDown nudTax;
        private Button btnAddItem;
        private Button btnRemoveItem;
        private Label lblTotalAmount;
        private Label lblStatus;
        private Button btnNew;
        private Button btnSave;
        private Button btnConfirm;
        private Button btnCancel;
        private Button btnRefresh;

        private List<Supplier> _suppliers;
        private List<Medicine> _medicines;
        private List<Purchase> _currentPurchases;
        private Purchase _currentPurchase;
        private List<PurchaseItem> _pendingItems;
        private int _selectedPurchaseId;
        private List<Purchase> _purchaseData;

        public PurchaseForm(IPurchaseService purchaseService, IMedicineService medicineService, ISupplierService supplierService)
        {
            _purchaseService = purchaseService;
            _medicineService = medicineService;
            _supplierService = supplierService;
            _pendingItems = new List<PurchaseItem>();
            InitializeComponents();
            LoadSuppliers();
            LoadMedicines();
            LoadPurchases();
        }

        private void InitializeComponents()
        {
            Text = "SmartMed - Purchases";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(860, 680);
            ShowIcon = false;

            int labelX = 16;
            int fieldX = 120;
            int fieldWidth = 160;
            int rowH = 30;
            int colGap = 230;

            Label lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(labelX, 12),
                Text = "Purchase Management"
            };
            Controls.Add(lblTitle);

            int gridY = 44;
            dgvPurchases = new DataGridView
            {
                Location = new Point(labelX, gridY),
                Width = 820,
                Height = 140,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvPurchases.SelectionChanged += DgvPurchases_SelectionChanged;
            Controls.Add(dgvPurchases);

            int detailY = gridY + 150;

            Label lblSupplier = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, detailY),
                Text = "Supplier:"
            };

            cboSupplier = new ComboBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, detailY - 3),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(lblSupplier);
            Controls.Add(cboSupplier);

            Label lblPurchaseNo = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, detailY),
                Text = "Purchase No:"
            };

            txtPurchaseNumber = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, detailY - 3),
                Width = fieldWidth
            };
            Controls.Add(lblPurchaseNo);
            Controls.Add(txtPurchaseNumber);

            int row2 = detailY + rowH;
            Label lblInvoice = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row2),
                Text = "Invoice No:"
            };

            txtInvoiceNumber = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row2 - 3),
                Width = fieldWidth
            };
            Controls.Add(lblInvoice);
            Controls.Add(txtInvoiceNumber);

            Label lblDate = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX + colGap, row2),
                Text = "Purchase Date:"
            };

            dtpPurchaseDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX + colGap, row2 - 3),
                Width = fieldWidth,
                Format = DateTimePickerFormat.Short
            };
            Controls.Add(lblDate);
            Controls.Add(dtpPurchaseDate);

            int row3 = row2 + rowH;
            Label lblRemarks = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, row3),
                Text = "Remarks:"
            };

            txtRemarks = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(fieldX, row3 - 3),
                Width = 350,
                Height = 50,
                Multiline = true
            };
            Controls.Add(lblRemarks);
            Controls.Add(txtRemarks);

            int itemsY = row3 + 60;
            Label lblItems = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(labelX, itemsY),
                Text = "Purchase Items"
            };
            Controls.Add(lblItems);

            int itemFormY = itemsY + 24;
            Label lblMedicine = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX, itemFormY),
                Text = "Medicine:"
            };

            cboMedicine = new ComboBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX, itemFormY - 2),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(lblMedicine);
            Controls.Add(cboMedicine);

            Label lblBatch = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(280, itemFormY),
                Text = "Batch:"
            };

            txtBatchNumber = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(320, itemFormY - 2),
                Width = 80
            };
            Controls.Add(lblBatch);
            Controls.Add(txtBatchNumber);

            Label lblExp = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(410, itemFormY),
                Text = "Expiry:"
            };

            dtpExpiry = new DateTimePicker
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(450, itemFormY - 2),
                Width = 90,
                Format = DateTimePickerFormat.Short
            };
            Controls.Add(lblExp);
            Controls.Add(dtpExpiry);

            int itemRow2 = itemFormY + rowH - 6;
            Label lblQty = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX, itemRow2),
                Text = "Qty:"
            };

            nudQuantity = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX, itemRow2 - 2),
                Width = 60,
                Minimum = 1,
                Maximum = 999999
            };
            Controls.Add(lblQty);
            Controls.Add(nudQuantity);

            Label lblPP = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(190, itemRow2),
                Text = "Pur.Price:"
            };

            nudPurchasePrice = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(250, itemRow2 - 2),
                Width = 80,
                DecimalPlaces = 2,
                Minimum = 0.01M,
                Maximum = 999999
            };
            Controls.Add(lblPP);
            Controls.Add(nudPurchasePrice);

            Label lblSP = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(340, itemRow2),
                Text = "Sell.Price:"
            };

            nudSellingPrice = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(400, itemRow2 - 2),
                Width = 80,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 999999
            };
            Controls.Add(lblSP);
            Controls.Add(nudSellingPrice);

            Label lblDisc = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(490, itemRow2),
                Text = "Disc%:"
            };

            nudDiscount = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(530, itemRow2 - 2),
                Width = 50,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100
            };
            Controls.Add(lblDisc);
            Controls.Add(nudDiscount);

            Label lblTax = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(590, itemRow2),
                Text = "Tax%:"
            };

            nudTax = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(625, itemRow2 - 2),
                Width = 50,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100
            };
            Controls.Add(lblTax);
            Controls.Add(nudTax);

            btnAddItem = new Button
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(690, itemRow2 - 2),
                Width = 60,
                Height = 24,
                Text = "Add"
            };
            btnAddItem.Click += BtnAddItem_Click;
            Controls.Add(btnAddItem);

            btnRemoveItem = new Button
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(755, itemRow2 - 2),
                Width = 60,
                Height = 24,
                Text = "Remove"
            };
            btnRemoveItem.Click += BtnRemoveItem_Click;
            Controls.Add(btnRemoveItem);

            int itemsGridY = itemRow2 + 30;
            dgvItems = new DataGridView
            {
                Location = new Point(labelX, itemsGridY),
                Width = 820,
                Height = 100,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            Controls.Add(dgvItems);

            int summaryY = itemsGridY + 110;
            Label lblTotal = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(labelX, summaryY),
                Text = "Total Amount:"
            };

            lblTotalAmount = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(120, summaryY),
                Text = "0.00"
            };
            Controls.Add(lblTotal);
            Controls.Add(lblTotalAmount);

            int buttonsY = summaryY + 30;
            btnNew = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelX, buttonsY),
                Width = 80,
                Height = 28,
                Text = "New"
            };
            btnNew.Click += BtnNew_Click;
            Controls.Add(btnNew);

            btnSave = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(104, buttonsY),
                Width = 80,
                Height = 28,
                Text = "Save"
            };
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            btnConfirm = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(192, buttonsY),
                Width = 80,
                Height = 28,
                Text = "Confirm"
            };
            btnConfirm.Click += BtnConfirm_Click;
            Controls.Add(btnConfirm);

            btnCancel = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(280, buttonsY),
                Width = 80,
                Height = 28,
                Text = "Cancel"
            };
            btnCancel.Click += BtnCancelPurchase_Click;
            Controls.Add(btnCancel);

            btnRefresh = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(368, buttonsY),
                Width = 80,
                Height = 28,
                Text = "Refresh"
            };
            btnRefresh.Click += (s, e) => LoadPurchases();
            Controls.Add(btnRefresh);

            lblStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Green,
                Location = new Point(labelX, buttonsY + 36),
                Width = 800,
                Text = "Ready"
            };
            Controls.Add(lblStatus);

            dtpPurchaseDate.Value = DateTime.Today;
        }

        private void LoadSuppliers()
        {
            try
            {
                OperationResult<List<Supplier>> result = _supplierService.GetAllSuppliers();
                if (result.IsSuccess)
                {
                    _suppliers = result.Data;
                    cboSupplier.Items.Clear();
                    cboSupplier.Items.Add("-- Select Supplier --");
                    foreach (Supplier s in _suppliers)
                        cboSupplier.Items.Add(s.SupplierName);
                    cboSupplier.SelectedIndex = 0;
                }
            }
            catch
            {
                _suppliers = new List<Supplier>();
            }
        }

        private void LoadMedicines()
        {
            try
            {
                OperationResult<List<Medicine>> result = _medicineService.GetAllMedicines();
                if (result.IsSuccess)
                {
                    _medicines = result.Data;
                    cboMedicine.Items.Clear();
                    cboMedicine.Items.Add("-- Select Medicine --");
                    foreach (Medicine m in _medicines)
                        cboMedicine.Items.Add(m.Name + (string.IsNullOrEmpty(m.Brand) ? "" : " (" + m.Brand + ")"));
                    cboMedicine.SelectedIndex = 0;
                }
            }
            catch
            {
                _medicines = new List<Medicine>();
            }
        }

        private void LoadPurchases()
        {
            try
            {
                OperationResult<List<Purchase>> result = _purchaseService.GetAllPurchases();
                if (result.IsSuccess)
                {
                    _currentPurchases = result.Data;
                    BindPurchases(_currentPurchases);
                    SetStatus($"Loaded {_currentPurchases.Count} purchase(s).", Color.Green);
                }
                else
                {
                    SetStatus(result.Message, Color.Red);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error loading purchases: {ex.Message}", Color.Red);
            }

            _selectedPurchaseId = 0;
        }

        private void BindPurchases(List<Purchase> purchases)
        {
            _purchaseData = purchases;

            var displayData = purchases.Select(p => new
            {
                p.Id,
                p.PurchaseNumber,
                p.PurchaseDate,
                Supplier = _suppliers?.Find(s => s.Id == p.SupplierId)?.SupplierName ?? "Unknown",
                p.InvoiceNumber,
                p.TotalAmount,
                Status = p.Status.ToString(),
                p.CreatedByUserId,
                p.ConfirmedDate
            }).ToList();

            dgvPurchases.DataSource = null;
            dgvPurchases.DataSource = displayData;

            if (dgvPurchases.Columns.Contains("Id"))
                dgvPurchases.Columns["Id"].Visible = false;
            if (dgvPurchases.Columns.Contains("CreatedByUserId"))
                dgvPurchases.Columns["CreatedByUserId"].Visible = false;
            if (dgvPurchases.Columns.Contains("ConfirmedDate"))
                dgvPurchases.Columns["ConfirmedDate"].Visible = false;
        }

        private void BindPendingItems()
        {
            var displayData = _pendingItems.Select(item =>
            {
                Medicine med = _medicines?.Find(m => m.Id == item.MedicineId);
                return new
                {
                    item.MedicineId,
                    Medicine = med?.Name ?? "Unknown",
                    item.BatchNumber,
                    item.ExpiryDate,
                    item.Quantity,
                    item.PurchasePrice,
                    item.SellingPrice,
                    item.DiscountPercent,
                    item.TaxPercent,
                    item.LineTotal
                };
            }).ToList();

            dgvItems.DataSource = null;
            dgvItems.DataSource = displayData;

            if (dgvItems.Columns.Contains("MedicineId"))
                dgvItems.Columns["MedicineId"].Visible = false;

            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = _pendingItems.Sum(i => i.LineTotal);
            lblTotalAmount.Text = total.ToString("N2");
        }

        private void ClearForm()
        {
            cboSupplier.SelectedIndex = 0;
            txtPurchaseNumber.Text = "";
            txtInvoiceNumber.Text = "";
            dtpPurchaseDate.Value = DateTime.Today;
            txtRemarks.Text = "";
            _pendingItems.Clear();
            BindPendingItems();
            _currentPurchase = null;
            _selectedPurchaseId = 0;
        }

        private void PopulatePurchase(Purchase purchase)
        {
            if (purchase == null) return;

            int supplierIndex = _suppliers.FindIndex(s => s.Id == purchase.SupplierId);
            cboSupplier.SelectedIndex = supplierIndex >= 0 ? supplierIndex + 1 : 0;
            txtPurchaseNumber.Text = purchase.PurchaseNumber;
            txtInvoiceNumber.Text = purchase.InvoiceNumber ?? "";
            dtpPurchaseDate.Value = purchase.PurchaseDate;
            txtRemarks.Text = purchase.Remarks ?? "";

            _currentPurchase = purchase;
        }

        private void DgvPurchases_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPurchases.SelectedRows.Count > 0)
            {
                int rowIndex = dgvPurchases.SelectedRows[0].Index;
                if (rowIndex >= 0 && rowIndex < _purchaseData.Count)
                {
                    Purchase purchase = _purchaseData[rowIndex];
                    _selectedPurchaseId = purchase.Id;
                    _currentPurchase = purchase;

                    OperationResult<Purchase> result = _purchaseService.GetPurchaseById(purchase.Id);
                    if (result.IsSuccess && result.Data != null)
                    {
                        _pendingItems = result.Data.Items ?? new List<PurchaseItem>();
                        PopulatePurchase(result.Data);
                        BindPendingItems();
                    }
                }
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
            txtPurchaseNumber.Text = "PUR-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            dtpPurchaseDate.Value = DateTime.Today;
            SetStatus("Enter purchase details and add items.", Color.Green);
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (cboMedicine.SelectedIndex <= 0)
            {
                SetStatus("Please select a medicine.", Color.Red);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBatchNumber.Text))
            {
                SetStatus("Please enter a batch number.", Color.Red);
                return;
            }

            int medicineIndex = cboMedicine.SelectedIndex - 1;
            Medicine medicine = _medicines[medicineIndex];

            PurchaseItem item = new PurchaseItem
            {
                MedicineId = medicine.Id,
                BatchNumber = txtBatchNumber.Text.Trim(),
                ExpiryDate = dtpExpiry.Value,
                Quantity = (int)nudQuantity.Value,
                PurchasePrice = nudPurchasePrice.Value,
                SellingPrice = nudSellingPrice.Value,
                DiscountPercent = nudDiscount.Value,
                TaxPercent = nudTax.Value
            };

            decimal subtotal = item.Quantity * item.PurchasePrice;
            decimal discountAmount = subtotal * (item.DiscountPercent / 100m);
            decimal taxAmount = (subtotal - discountAmount) * (item.TaxPercent / 100m);
            item.LineTotal = subtotal - discountAmount + taxAmount;

            _pendingItems.Add(item);
            BindPendingItems();

            nudQuantity.Value = 1;
            nudPurchasePrice.Value = 0;
            nudSellingPrice.Value = 0;
            nudDiscount.Value = 0;
            nudTax.Value = 0;
            txtBatchNumber.Text = "";
            cboMedicine.SelectedIndex = 0;

            SetStatus($"Item added. {_pendingItems.Count} item(s) in list.", Color.Green);
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count == 0 || _pendingItems.Count == 0)
            {
                SetStatus("No item selected to remove.", Color.Red);
                return;
            }

            int rowIndex = dgvItems.SelectedRows[0].Index;
            if (rowIndex >= 0 && rowIndex < _pendingItems.Count)
            {
                _pendingItems.RemoveAt(rowIndex);
                BindPendingItems();
                SetStatus("Item removed.", Color.Green);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cboSupplier.SelectedIndex <= 0)
            {
                SetStatus("Please select a supplier.", Color.Red);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPurchaseNumber.Text))
            {
                SetStatus("Please enter a purchase number.", Color.Red);
                return;
            }

            if (_pendingItems.Count == 0)
            {
                SetStatus("Please add at least one item.", Color.Red);
                return;
            }

            Supplier supplier = _suppliers[cboSupplier.SelectedIndex - 1];

            Purchase purchase = new Purchase
            {
                PurchaseNumber = txtPurchaseNumber.Text.Trim(),
                PurchaseDate = dtpPurchaseDate.Value,
                SupplierId = supplier.Id,
                InvoiceNumber = txtInvoiceNumber.Text.Trim(),
                Remarks = txtRemarks.Text.Trim(),
                Items = new List<PurchaseItem>(_pendingItems)
            };

            OperationResult<int> result = _purchaseService.CreatePurchase(purchase);
            if (result.IsSuccess)
            {
                ClearForm();
                LoadPurchases();
                SetStatus("Purchase created successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (_selectedPurchaseId <= 0)
            {
                SetStatus("Please select a purchase to confirm.", Color.Red);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to confirm this purchase? Stock will be updated.",
                "Confirm Purchase",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            OperationResult result = _purchaseService.ConfirmPurchase(_selectedPurchaseId);
            if (result.IsSuccess)
            {
                LoadPurchases();
                SetStatus("Purchase confirmed successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BtnCancelPurchase_Click(object sender, EventArgs e)
        {
            if (_selectedPurchaseId <= 0)
            {
                SetStatus("Please select a purchase to cancel.", Color.Red);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to cancel this purchase?",
                "Cancel Purchase",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            OperationResult result = _purchaseService.CancelPurchase(_selectedPurchaseId);
            if (result.IsSuccess)
            {
                LoadPurchases();
                SetStatus("Purchase cancelled.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void SetStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
