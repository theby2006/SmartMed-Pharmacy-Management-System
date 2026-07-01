using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;
using SmartMed.Models.Session;
using SmartMed.UI.Components;

namespace SmartMed.UI.Forms
{
    public class SalesForm : Form
    {
        private readonly ISalesService _salesService;
        private readonly IPaymentService _paymentService;
        private readonly IPricingService _pricingService;
        private readonly IMedicineService _medicineService;
        private readonly IInventoryService _inventoryService;
        private readonly ISessionManager _sessionManager;
        private readonly ISaleNumberGenerator _saleNumberGenerator;

        private TextBox txtSaleNumber;
        private DateTimePicker dtpSaleDate;
        private TextBox txtCashier;
        private ComboBox cboCustomerType;
        private TextBox txtCustomerName;
        private TextBox txtCustomerPhone;
        private TextBox txtBarcodeSearch;
        private TextBox txtMedicineSearch;
        private Button btnSearchMedicine;
        private DataGridView dgvSearchResults;
        private DataGridView dgvBatches;
        private NumericUpDown nudItemQuantity;
        private NumericUpDown nudItemDiscount;
        private NumericUpDown nudItemTax;
        private Button btnAddItem;
        private Button btnRemoveItem;
        private Button btnClearCart;
        private DataGridView dgvCart;
        private Label lblSubTotal;
        private NumericUpDown nudDiscountPercent;
        private NumericUpDown nudTaxPercent;
        private Label lblGrandTotal;
        private NumericUpDown nudAmountPaid;
        private Label lblBalance;
        private Button btnCompleteSale;
        private Button btnCancelSale;
        private Button btnPrintInvoice;
        private Label lblStatus;
        private CheckBox chkCustomerLookup;
        private NumericUpDown nudCustomerId;

        private List<Medicine> _searchResults;
        private List<StockBatch> _currentBatches;
        private Medicine _selectedMedicine;
        private StockBatch _selectedBatch;
        private List<CartItem> _cartItems;
        private int _completedSaleId;
        private bool _isProcessing;

        public SalesForm(
            ISalesService salesService,
            IPaymentService paymentService,
            IPricingService pricingService,
            IMedicineService medicineService,
            IInventoryService inventoryService,
            ISessionManager sessionManager,
            ISaleNumberGenerator saleNumberGenerator)
        {
            _salesService = salesService;
            _paymentService = paymentService;
            _pricingService = pricingService;
            _medicineService = medicineService;
            _inventoryService = inventoryService;
            _sessionManager = sessionManager;
            _saleNumberGenerator = saleNumberGenerator;
            _cartItems = new List<CartItem>();
            InitializeComponents();
            LoadDefaults();
        }

        private void InitializeComponents()
        {
            Text = "SmartMed - Point of Sale";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(960, 760);
            ShowIcon = false;

            int labelX = 12;
            int fieldX = 100;
            int fieldW = 150;
            int rowH = 28;
            int colGap = 240;

            Label lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(labelX, 10),
                Text = "New Sale"
            };
            Controls.Add(lblTitle);

            int y = 40;
            Label lblSaleNo = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX, y),
                Text = "Sale #:"
            };

            txtSaleNumber = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX, y - 2),
                Width = fieldW,
                ReadOnly = true,
                BackColor = Color.WhiteSmoke
            };
            Controls.Add(lblSaleNo);
            Controls.Add(txtSaleNumber);

            Label lblDate = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX + colGap, y),
                Text = "Date:"
            };

            dtpSaleDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX + colGap, y - 2),
                Width = fieldW,
                Format = DateTimePickerFormat.Short,
                Enabled = false
            };
            Controls.Add(lblDate);
            Controls.Add(dtpSaleDate);

            Label lblCashier = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX + colGap * 2, y),
                Text = "Cashier:"
            };

            txtCashier = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX + colGap * 2, y - 2),
                Width = fieldW,
                ReadOnly = true,
                BackColor = Color.WhiteSmoke
            };
            Controls.Add(lblCashier);
            Controls.Add(txtCashier);

            int custY = y + rowH;
            Label lblCustType = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX, custY),
                Text = "Customer Type:"
            };

            cboCustomerType = new ComboBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX, custY - 2),
                Width = fieldW,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboCustomerType.Items.AddRange(new[] { "Walk-in", "Regular", "Corporate", "Insurance" });
            cboCustomerType.SelectedIndex = 0;
            Controls.Add(lblCustType);
            Controls.Add(cboCustomerType);

            Label lblCustName = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX + colGap, custY),
                Text = "Customer Name:"
            };

            txtCustomerName = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX + colGap, custY - 2),
                Width = 130
            };
            Controls.Add(lblCustName);
            Controls.Add(txtCustomerName);

            Label lblCustPhone = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX + colGap + 210, custY),
                Text = "Customer Phone:"
            };

            txtCustomerPhone = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX + colGap + 210, custY - 2),
                Width = 120
            };
            Controls.Add(lblCustPhone);
            Controls.Add(txtCustomerPhone);

            chkCustomerLookup = new CheckBox
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 8F),
                Location = new Point(fieldX + colGap + 340, custY),
                Text = "Has Customer ID"
            };
            chkCustomerLookup.CheckedChanged += (s, e) => nudCustomerId.Enabled = chkCustomerLookup.Checked;
            Controls.Add(chkCustomerLookup);

            nudCustomerId = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX + colGap + 340, custY - 2),
                Width = 60,
                Minimum = 1,
                Maximum = 999999,
                Enabled = false,
                Visible = false
            };
            Controls.Add(nudCustomerId);

            int searchY = custY + rowH + 4;
            Label lblBarcode = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX, searchY),
                Text = "Barcode:"
            };

            txtBarcodeSearch = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX, searchY - 2),
                Width = 120
            };
            txtBarcodeSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) PerformBarcodeSearch(); };
            Controls.Add(lblBarcode);
            Controls.Add(txtBarcodeSearch);

            Label lblMedSearch = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX + colGap, searchY),
                Text = "Medicine:"
            };

            txtMedicineSearch = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX + colGap, searchY - 2),
                Width = 150
            };
            txtMedicineSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) PerformMedicineSearch(); };
            Controls.Add(lblMedSearch);
            Controls.Add(txtMedicineSearch);

            btnSearchMedicine = new Button
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(fieldX + colGap + 156, searchY - 2),
                Width = 60,
                Height = 24,
                Text = "Search"
            };
            btnSearchMedicine.Click += (s, e) => PerformMedicineSearch();
            Controls.Add(btnSearchMedicine);

            int resultsGridY = searchY + rowH;
            dgvSearchResults = new DataGridView
            {
                Location = new Point(labelX, resultsGridY),
                Width = 580,
                Height = 100,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvSearchResults.SelectionChanged += DgvSearchResults_SelectionChanged;
            Controls.Add(dgvSearchResults);

            int batchGridY = resultsGridY + 108;
            Label lblBatches = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(labelX, batchGridY),
                Text = "Available Batches"
            };
            Controls.Add(lblBatches);

            dgvBatches = new DataGridView
            {
                Location = new Point(labelX, batchGridY + 18),
                Width = 580,
                Height = 80,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvBatches.SelectionChanged += DgvBatches_SelectionChanged;
            Controls.Add(dgvBatches);

            int itemEntryY = batchGridY + 106;
            Label lblQty = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(labelX, itemEntryY),
                Text = "Qty:"
            };

            nudItemQuantity = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(40, itemEntryY - 2),
                Width = 60,
                Minimum = 1,
                Maximum = 99999,
                Value = 1
            };
            Controls.Add(lblQty);
            Controls.Add(nudItemQuantity);

            Label lblItemDisc = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(110, itemEntryY),
                Text = "Disc%:"
            };

            nudItemDiscount = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(155, itemEntryY - 2),
                Width = 60,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100
            };
            Controls.Add(lblItemDisc);
            Controls.Add(nudItemDiscount);

            Label lblItemTax = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(225, itemEntryY),
                Text = "Tax%:"
            };

            nudItemTax = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(270, itemEntryY - 2),
                Width = 60,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100
            };
            Controls.Add(lblItemTax);
            Controls.Add(nudItemTax);

            btnAddItem = new Button
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(345, itemEntryY - 2),
                Width = 70,
                Height = 24,
                Text = "Add Item"
            };
            btnAddItem.Click += BtnAddItem_Click;
            Controls.Add(btnAddItem);

            btnRemoveItem = new Button
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(420, itemEntryY - 2),
                Width = 80,
                Height = 24,
                Text = "Remove Item"
            };
            btnRemoveItem.Click += BtnRemoveItem_Click;
            Controls.Add(btnRemoveItem);

            btnClearCart = new Button
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(505, itemEntryY - 2),
                Width = 70,
                Height = 24,
                Text = "Clear Cart"
            };
            btnClearCart.Click += BtnClearCart_Click;
            Controls.Add(btnClearCart);

            int cartY = itemEntryY + rowH;
            Label lblCart = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(labelX, cartY),
                Text = "Cart"
            };
            Controls.Add(lblCart);

            dgvCart = new DataGridView
            {
                Location = new Point(labelX, cartY + 18),
                Width = 580,
                Height = 130,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            Controls.Add(dgvCart);

            int paymentX = 610;
            int payY = resultsGridY;
            Label lblPayment = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(paymentX, payY),
                Text = "Payment Summary"
            };
            Controls.Add(lblPayment);

            payY += 30;
            Label lblSub = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX, payY),
                Text = "Sub Total:"
            };

            lblSubTotal = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(paymentX + 100, payY),
                Text = "0.00"
            };
            Controls.Add(lblSub);
            Controls.Add(lblSubTotal);

            payY += rowH;
            Label lblDisc = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX, payY),
                Text = "Discount%:"
            };

            nudDiscountPercent = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX + 100, payY - 2),
                Width = 80,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            nudDiscountPercent.ValueChanged += (s, e) => RecalculateTotals();
            Controls.Add(lblDisc);
            Controls.Add(nudDiscountPercent);

            payY += rowH;
            Label lblTax = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX, payY),
                Text = "Tax%:"
            };

            nudTaxPercent = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX + 100, payY - 2),
                Width = 80,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            nudTaxPercent.ValueChanged += (s, e) => RecalculateTotals();
            Controls.Add(lblTax);
            Controls.Add(nudTaxPercent);

            payY += rowH;
            Label lblGrand = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(paymentX, payY),
                Text = "Grand Total:"
            };

            lblGrandTotal = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Location = new Point(paymentX + 110, payY),
                Text = "0.00"
            };
            Controls.Add(lblGrand);
            Controls.Add(lblGrandTotal);

            payY += rowH + 8;
            Label lblPaymentMethod = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(paymentX, payY),
                Text = "Payment (Cash)"
            };
            Controls.Add(lblPaymentMethod);

            payY += rowH;
            Label lblPaid = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX, payY),
                Text = "Amount Paid:"
            };

            nudAmountPaid = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX + 100, payY - 2),
                Width = 120,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 9999999
            };
            nudAmountPaid.ValueChanged += (s, e) => RecalculateBalance();
            Controls.Add(lblPaid);
            Controls.Add(nudAmountPaid);

            payY += rowH;
            Label lblBal = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX, payY),
                Text = "Balance:"
            };

            lblBalance = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Green,
                Location = new Point(paymentX + 100, payY),
                Text = "0.00"
            };
            Controls.Add(lblBal);
            Controls.Add(lblBalance);

            int btnY = payY + rowH + 10;
            btnCompleteSale = new Button
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Location = new Point(paymentX, btnY),
                Width = 110,
                Height = 30,
                Text = "Complete Sale"
            };
            btnCompleteSale.Click += BtnCompleteSale_Click;
            Controls.Add(btnCompleteSale);

            btnCancelSale = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX + 120, btnY),
                Width = 100,
                Height = 30,
                Text = "Cancel Sale"
            };
            btnCancelSale.Click += BtnCancelSale_Click;
            Controls.Add(btnCancelSale);

            btnPrintInvoice = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(paymentX + 230, btnY),
                Width = 100,
                Height = 30,
                Text = "Print Invoice"
            };
            btnPrintInvoice.Enabled = false;
            Controls.Add(btnPrintInvoice);

            int statusY = btnY + 42;
            lblStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Green,
                Location = new Point(labelX, statusY),
                Width = 920,
                Text = "Ready"
            };
            Controls.Add(lblStatus);
        }

        private void LoadDefaults()
        {
            dtpSaleDate.Value = DateTime.Today;
            SessionContext session = _sessionManager.CurrentSession;
            txtCashier.Text = session != null ? session.DisplayName : "Unknown";
            GenerateSaleNumber();
        }

        private void GenerateSaleNumber()
        {
            OperationResult<string> result = _saleNumberGenerator.GenerateNextNumber();
            if (result.IsSuccess)
            {
                txtSaleNumber.Text = result.Data;
            }
            else
            {
                txtSaleNumber.Text = "SAL-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                SetStatus("Warning: Using fallback sale number. " + result.Message, Color.Orange);
            }
        }

        private void PerformBarcodeSearch()
        {
            string barcode = txtBarcodeSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            OperationResult<List<Medicine>> result = _medicineService.SearchMedicines(barcode);
            if (result.IsSuccess && result.Data.Count > 0)
            {
                _searchResults = result.Data;
                BindSearchResults(_searchResults);
                _selectedMedicine = _searchResults[0];
                dgvSearchResults.ClearSelection();
                if (dgvSearchResults.Rows.Count > 0)
                    dgvSearchResults.Rows[0].Selected = true;

                LoadBatches(_selectedMedicine.Id);

                if (_currentBatches != null && _currentBatches.Count > 0)
                {
                    _selectedBatch = _currentBatches[0];
                    dgvBatches.ClearSelection();
                    if (dgvBatches.Rows.Count > 0)
                        dgvBatches.Rows[0].Selected = true;

                    AutoAddToCart();
                }
                else
                {
                    SetStatus($"Found {_selectedMedicine.Name} but no stock available.", Color.Red);
                }
            }
            else
            {
                SetStatus("No medicine found for this barcode.", Color.Red);
            }
        }

        private void AutoAddToCart()
        {
            if (_selectedMedicine == null || _selectedBatch == null) return;

            if (IsDuplicateItem(_selectedMedicine.Id, _selectedBatch.Id))
            {
                SetStatus($"'{_selectedMedicine.Name}' from batch {_selectedBatch.BatchNumber} is already in cart.", Color.Red);
                return;
            }

            if (_selectedBatch.ExpiryDate <= DateTime.Today)
            {
                SetStatus($"Batch {_selectedBatch.BatchNumber} is expired ({_selectedBatch.ExpiryDate:yyyy-MM-dd}).", Color.Red);
                return;
            }

            AddItemToCart(1);
            txtBarcodeSearch.Clear();
        }

        private bool IsDuplicateItem(int medicineId, int batchId)
        {
            return _cartItems.Any(c => c.MedicineId == medicineId && c.StockBatchId == batchId);
        }

        private void PerformMedicineSearch()
        {
            string keyword = txtMedicineSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword)) return;

            OperationResult<List<Medicine>> result = _medicineService.SearchMedicines(keyword);
            if (result.IsSuccess)
            {
                _searchResults = result.Data;
                BindSearchResults(_searchResults);
                SetStatus($"Found {_searchResults.Count} medicine(s).", result.Data.Count > 0 ? Color.Green : Color.Red);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BindSearchResults(List<Medicine> medicines)
        {
            var display = medicines.Select(m => new
            {
                m.Id,
                m.Name,
                m.Brand,
                DosageForm = m.DosageForm.ToString(),
                m.Strength,
                Stock = m.StockQuantity,
                m.UnitPrice
            }).ToList();

            dgvSearchResults.DataSource = null;
            dgvSearchResults.DataSource = display;

            if (dgvSearchResults.Columns.Contains("Id"))
                dgvSearchResults.Columns["Id"].Visible = false;
        }

        private void DgvSearchResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSearchResults.SelectedRows.Count == 0 || _searchResults == null) return;

            int rowIndex = dgvSearchResults.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _searchResults.Count) return;

            _selectedMedicine = _searchResults[rowIndex];
            LoadBatches(_selectedMedicine.Id);
        }

        private void LoadBatches(int medicineId)
        {
            OperationResult<List<StockBatch>> result = _inventoryService.GetStockBatches(medicineId);
            if (result.IsSuccess)
            {
                _currentBatches = result.Data
                    .Where(b => b.CurrentQuantity > 0)
                    .OrderBy(b => b.ExpiryDate)
                    .ToList();

                var display = _currentBatches
                    .Select(b => new
                    {
                        b.Id,
                        b.BatchNumber,
                        Expiry = b.ExpiryDate.ToString("yyyy-MM-dd"),
                        Available = b.CurrentQuantity,
                        b.SellingPrice,
                        IsExpired = b.ExpiryDate <= DateTime.Today
                    }).ToList();

                dgvBatches.DataSource = null;
                dgvBatches.DataSource = display;

                if (dgvBatches.Columns.Contains("Id"))
                    dgvBatches.Columns["Id"].Visible = false;
                if (dgvBatches.Columns.Contains("IsExpired"))
                    dgvBatches.Columns["IsExpired"].Visible = false;
            }
            else
            {
                dgvBatches.DataSource = null;
                _currentBatches = null;
            }
        }

        private void DgvBatches_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBatches.SelectedRows.Count == 0 || _currentBatches == null) return;

            int rowIndex = dgvBatches.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _currentBatches.Count)
            {
                _selectedBatch = null;
                return;
            }

            _selectedBatch = _currentBatches[rowIndex];
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (_selectedMedicine == null)
            {
                SetStatus("Please select a medicine from search results.", Color.Red);
                return;
            }

            if (_selectedBatch == null)
            {
                SetStatus("Please select a batch.", Color.Red);
                return;
            }

            if (IsDuplicateItem(_selectedMedicine.Id, _selectedBatch.Id))
            {
                SetStatus($"'{_selectedMedicine.Name}' from batch {_selectedBatch.BatchNumber} is already in cart.", Color.Red);
                return;
            }

            if (_selectedBatch.ExpiryDate <= DateTime.Today)
            {
                SetStatus($"Batch {_selectedBatch.BatchNumber} is expired ({_selectedBatch.ExpiryDate:yyyy-MM-dd}).", Color.Red);
                return;
            }

            int qty = (int)nudItemQuantity.Value;
            if (qty <= 0)
            {
                SetStatus("Quantity must be greater than zero.", Color.Red);
                return;
            }

            if (qty > _selectedBatch.CurrentQuantity)
            {
                SetStatus($"Only {_selectedBatch.CurrentQuantity} available in selected batch.", Color.Red);
                return;
            }

            AddItemToCart(qty);

            nudItemQuantity.Value = 1;
            nudItemDiscount.Value = 0;
            nudItemTax.Value = 0;

            SetStatus($"Added {_selectedMedicine.Name} x{qty} to cart.", Color.Green);
        }

        private void AddItemToCart(int quantity)
        {
            decimal lineTotal = _pricingService.CalculateLineTotal(
                quantity, _selectedBatch.SellingPrice, nudItemDiscount.Value, nudItemTax.Value);

            CartItem item = new CartItem
            {
                MedicineId = _selectedMedicine.Id,
                MedicineName = _selectedMedicine.Name,
                StockBatchId = _selectedBatch.Id,
                BatchNumber = _selectedBatch.BatchNumber,
                ExpiryDate = _selectedBatch.ExpiryDate,
                Quantity = quantity,
                UnitPrice = _selectedBatch.SellingPrice,
                DiscountPercent = nudItemDiscount.Value,
                TaxPercent = nudItemTax.Value,
                LineTotal = lineTotal
            };

            _cartItems.Add(item);
            BindCart();
            RecalculateTotals();
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0 || _cartItems.Count == 0)
            {
                SetStatus("No item selected to remove.", Color.Red);
                return;
            }

            int rowIndex = dgvCart.SelectedRows[0].Index;
            if (rowIndex >= 0 && rowIndex < _cartItems.Count)
            {
                _cartItems.RemoveAt(rowIndex);
                BindCart();
                RecalculateTotals();
                SetStatus("Item removed from cart.", Color.Green);
            }
        }

        private void BtnClearCart_Click(object sender, EventArgs e)
        {
            if (_cartItems.Count == 0) return;

            DialogResult confirm = MessageBox.Show(
                "Clear all items from cart?",
                "Clear Cart",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                _cartItems.Clear();
                BindCart();
                RecalculateTotals();
                SetStatus("Cart cleared.", Color.Green);
            }
        }

        private void BindCart()
        {
            var display = _cartItems.Select(c => new
            {
                c.MedicineName,
                c.BatchNumber,
                Expiry = c.ExpiryDate.ToString("yyyy-MM-dd"),
                c.Quantity,
                UnitPrice = c.UnitPrice.ToString("N2"),
                Discount = c.DiscountPercent.ToString("N1") + "%",
                Tax = c.TaxPercent.ToString("N1") + "%",
                Total = c.LineTotal.ToString("N2"),
                c.MedicineId,
                c.StockBatchId
            }).ToList();

            dgvCart.DataSource = null;
            dgvCart.DataSource = display;

            if (dgvCart.Columns.Contains("MedicineId"))
                dgvCart.Columns["MedicineId"].Visible = false;
            if (dgvCart.Columns.Contains("StockBatchId"))
                dgvCart.Columns["StockBatchId"].Visible = false;
        }

        private void RecalculateTotals()
        {
            if (_isProcessing) return;

            decimal subTotal = _cartItems.Sum(c => _pricingService.CalculateSubTotal(c.Quantity, c.UnitPrice));
            decimal discountAmount = _pricingService.CalculateDiscountAmount(subTotal, nudDiscountPercent.Value);
            decimal taxAmount = _pricingService.CalculateTaxAmount(subTotal - discountAmount, nudTaxPercent.Value);
            decimal grandTotal = _pricingService.CalculateGrandTotal(subTotal, discountAmount, taxAmount);

            lblSubTotal.Text = subTotal.ToString("N2");
            lblGrandTotal.Text = grandTotal.ToString("N2");

            RecalculateBalance();
        }

        private void RecalculateBalance()
        {
            if (_isProcessing) return;

            decimal grandTotal = 0;
            decimal.TryParse(lblGrandTotal.Text, out grandTotal);

            decimal amountPaid = nudAmountPaid.Value;
            decimal balance = _pricingService.CalculateChange(amountPaid, grandTotal);

            lblBalance.Text = balance.ToString("N2");
            lblBalance.ForeColor = balance >= 0 ? Color.Green : Color.Red;
        }

        private void BtnCompleteSale_Click(object sender, EventArgs e)
        {
            if (_cartItems.Count == 0)
            {
                SetStatus("Cart is empty. Add items before completing sale.", Color.Red);
                return;
            }

            decimal grandTotal = 0;
            decimal.TryParse(lblGrandTotal.Text, out grandTotal);

            if (grandTotal <= 0)
            {
                SetStatus("Grand total must be greater than zero.", Color.Red);
                return;
            }

            if (nudAmountPaid.Value < grandTotal)
            {
                SetStatus($"Amount paid ({nudAmountPaid.Value:N2}) is less than grand total ({grandTotal:N2}).", Color.Red);
                return;
            }

            foreach (CartItem cartItem in _cartItems)
            {
                if (cartItem.Quantity <= 0)
                {
                    SetStatus($"Invalid quantity for '{cartItem.MedicineName}'.", Color.Red);
                    return;
                }

                if (cartItem.ExpiryDate <= DateTime.Today)
                {
                    SetStatus($"'{cartItem.MedicineName}' batch {cartItem.BatchNumber} is expired.", Color.Red);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(txtSaleNumber.Text))
            {
                SetStatus("Sale number is not available. Cannot complete sale.", Color.Red);
                return;
            }

            _isProcessing = true;
            btnCompleteSale.Enabled = false;

            try
            {
                DialogResult confirm = MessageBox.Show(
                    $"Complete sale for {lblGrandTotal.Text}?",
                    "Confirm Sale",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                CompleteSale();
            }
            finally
            {
                _isProcessing = false;
                btnCompleteSale.Enabled = true;
            }
        }

        private void CompleteSale()
        {
            SessionContext session = _sessionManager.CurrentSession;
            decimal subTotal = _cartItems.Sum(c => _pricingService.CalculateSubTotal(c.Quantity, c.UnitPrice));

            Sale sale = new Sale
            {
                SaleNumber = txtSaleNumber.Text.Trim(),
                SaleDate = dtpSaleDate.Value,
                CashierId = session?.UserId ?? 0,
                CustomerId = chkCustomerLookup.Checked ? (int)nudCustomerId.Value : (int?)null,
                CustomerName = txtCustomerName.Text.Trim(),
                CustomerPhone = txtCustomerPhone.Text.Trim(),
                DiscountPercent = nudDiscountPercent.Value,
                TaxPercent = nudTaxPercent.Value,
                Notes = $"CustomerType: {cboCustomerType.SelectedItem}"
            };

            List<SaleItem> items = _cartItems.Select(c => new SaleItem
            {
                MedicineId = c.MedicineId,
                StockBatchId = c.StockBatchId,
                BatchNumber = c.BatchNumber,
                ExpiryDate = c.ExpiryDate,
                Quantity = c.Quantity,
                SellingPrice = c.UnitPrice,
                DiscountPercent = c.DiscountPercent,
                TaxPercent = c.TaxPercent,
                LineTotal = c.LineTotal
            }).ToList();

            decimal grandTotal = 0;
            decimal.TryParse(lblGrandTotal.Text, out grandTotal);

            Payment payment = new Payment
            {
                PaymentMethod = PaymentMethod.Cash,
                AmountPaid = nudAmountPaid.Value,
                ChangeAmount = _pricingService.CalculateChange(nudAmountPaid.Value, grandTotal)
            };

            OperationResult<int> result = _salesService.CreateSale(sale, items, payment);
            if (result.IsSuccess)
            {
                _completedSaleId = result.Data;
                SetStatus($"Sale #{txtSaleNumber.Text} completed successfully!", Color.Green);
                btnPrintInvoice.Enabled = true;

                bool printInvoice = MessageBox.Show(
                    $"Sale #{txtSaleNumber.Text} completed. Total: {lblGrandTotal.Text}\n\nPrint invoice?",
                    "Sale Complete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information) == DialogResult.Yes;

                if (printInvoice)
                {
                    PrintInvoice();
                }

                RefreshInventoryView();
                ResetForm();
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void RefreshInventoryView()
        {
            if (!string.IsNullOrWhiteSpace(txtMedicineSearch.Text))
            {
                PerformMedicineSearch();
            }

            if (_selectedMedicine != null)
            {
                LoadBatches(_selectedMedicine.Id);
            }
        }

        private void BtnCancelSale_Click(object sender, EventArgs e)
        {
            if (_cartItems.Count == 0 && string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Cancel this sale? All entered data will be lost.",
                "Cancel Sale",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void PrintInvoice()
        {
            decimal subTotal = _cartItems.Sum(c => _pricingService.CalculateSubTotal(c.Quantity, c.UnitPrice));
            decimal grandTotal = 0;
            decimal.TryParse(lblGrandTotal.Text, out grandTotal);

            var printItems = _cartItems.Select(c => new PrintLineItem
            {
                MedicineName = c.MedicineName,
                Quantity = c.Quantity,
                UnitPrice = c.UnitPrice,
                DiscountPercent = c.DiscountPercent,
                TaxPercent = c.TaxPercent,
                LineTotal = c.LineTotal
            }).ToList();

            using (SalePrintDocument doc = new SalePrintDocument(
                txtSaleNumber.Text,
                dtpSaleDate.Value.ToString("yyyy-MM-dd"),
                txtCashier.Text,
                txtCustomerName.Text,
                txtCustomerPhone.Text,
                printItems,
                subTotal,
                nudDiscountPercent.Value,
                nudTaxPercent.Value,
                grandTotal,
                nudAmountPaid.Value,
                decimal.Parse(lblBalance.Text),
                _pricingService))
            {
                using (PrintPreviewDialog preview = new PrintPreviewDialog
                {
                    Document = doc,
                    Width = 800,
                    Height = 600,
                    Text = $"Print Preview - Invoice #{txtSaleNumber.Text}"
                })
                {
                    preview.ShowDialog(this);
                }
            }
        }

        private void ResetForm()
        {
            _cartItems.Clear();
            _selectedMedicine = null;
            _selectedBatch = null;
            _searchResults = null;
            _currentBatches = null;
            _completedSaleId = 0;

            txtCustomerName.Text = "";
            txtCustomerPhone.Text = "";
            txtBarcodeSearch.Text = "";
            txtMedicineSearch.Text = "";
            nudDiscountPercent.Value = 0;
            nudTaxPercent.Value = 0;
            nudAmountPaid.Value = 0;
            nudItemQuantity.Value = 1;
            nudItemDiscount.Value = 0;
            nudItemTax.Value = 0;
            cboCustomerType.SelectedIndex = 0;
            chkCustomerLookup.Checked = false;
            nudCustomerId.Value = 1;

            dgvSearchResults.DataSource = null;
            dgvBatches.DataSource = null;
            dgvCart.DataSource = null;

            LoadDefaults();
            RecalculateTotals();
            RecalculateBalance();
        }

        private void SetStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private class CartItem
        {
            public int MedicineId { get; set; }
            public string MedicineName { get; set; }
            public int StockBatchId { get; set; }
            public string BatchNumber { get; set; }
            public DateTime ExpiryDate { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountPercent { get; set; }
            public decimal TaxPercent { get; set; }
            public decimal LineTotal { get; set; }
        }
    }
}
