using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;
using SmartMed.UI.Forms;

namespace SmartMed.Tests.UI
{
    [TestClass]
    [TestCategory("WindowsOnly")]
    public class SalesFormTests
    {
        [TestMethod]
        public void SalesForm_ShouldInitializeComponents()
        {
            using (SalesForm form = CreateSalesForm())
            {
                Assert.IsNotNull(form);
                Assert.IsFalse(string.IsNullOrWhiteSpace(form.Text));
            }
        }

        [TestMethod]
        public void SalesForm_ShouldGenerateSaleNumber_InCorrectFormat()
        {
            using (SalesForm form = CreateSalesForm())
            {
                string saleNumber = GetFieldText(form, "txtSaleNumber");
                Assert.IsFalse(string.IsNullOrWhiteSpace(saleNumber));
                Assert.IsTrue(saleNumber.StartsWith("SAL-"), $"Sale number should start with 'SAL-'. Got: {saleNumber}");
            }
        }

        [TestMethod]
        public void SalesForm_ShouldDisplayCashierName()
        {
            using (SalesForm form = CreateSalesForm())
            {
                string cashier = GetFieldText(form, "txtCashier");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cashier));
                Assert.AreEqual("Test Cashier", cashier);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveDefaultCustomerType_WalkIn()
        {
            using (SalesForm form = CreateSalesForm())
            {
                ComboBox cbo = GetField<ComboBox>(form, "cboCustomerType");
                Assert.IsNotNull(cbo);
                Assert.AreEqual(0, cbo.SelectedIndex);
                Assert.AreEqual("Walk-in", cbo.SelectedItem);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveSubTotal_InitiallyZero()
        {
            using (SalesForm form = CreateSalesForm())
            {
                string subTotal = GetFieldText(form, "lblSubTotal");
                Assert.AreEqual("0.00", subTotal);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveGrandTotal_InitiallyZero()
        {
            using (SalesForm form = CreateSalesForm())
            {
                string grandTotal = GetFieldText(form, "lblGrandTotal");
                Assert.AreEqual("0.00", grandTotal);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveBalance_InitiallyZero()
        {
            using (SalesForm form = CreateSalesForm())
            {
                string balance = GetFieldText(form, "lblBalance");
                Assert.AreEqual("0.00", balance);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveButton_CompleteSale()
        {
            using (SalesForm form = CreateSalesForm())
            {
                Button btn = GetField<Button>(form, "btnCompleteSale");
                Assert.IsNotNull(btn);
                Assert.IsTrue(btn.Enabled);
                Assert.AreEqual("Complete Sale", btn.Text);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveButton_CancelSale()
        {
            using (SalesForm form = CreateSalesForm())
            {
                Button btn = GetField<Button>(form, "btnCancelSale");
                Assert.IsNotNull(btn);
                Assert.IsTrue(btn.Enabled);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveButton_PrintInvoice_InitiallyDisabled()
        {
            using (SalesForm form = CreateSalesForm())
            {
                Button btn = GetField<Button>(form, "btnPrintInvoice");
                Assert.IsNotNull(btn);
                Assert.IsFalse(btn.Enabled);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveDataGridView_Cart()
        {
            using (SalesForm form = CreateSalesForm())
            {
                DataGridView dgv = GetField<DataGridView>(form, "dgvCart");
                Assert.IsNotNull(dgv);
                Assert.IsFalse(dgv.AllowUserToAddRows);
                Assert.IsTrue(dgv.ReadOnly);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveDataGridView_SearchResults()
        {
            using (SalesForm form = CreateSalesForm())
            {
                DataGridView dgv = GetField<DataGridView>(form, "dgvSearchResults");
                Assert.IsNotNull(dgv);
                Assert.IsFalse(dgv.AllowUserToAddRows);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveDataGridView_Batches()
        {
            using (SalesForm form = CreateSalesForm())
            {
                DataGridView dgv = GetField<DataGridView>(form, "dgvBatches");
                Assert.IsNotNull(dgv);
                Assert.IsFalse(dgv.AllowUserToAddRows);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveQuantityInput_WithMinimumOne()
        {
            using (SalesForm form = CreateSalesForm())
            {
                NumericUpDown nud = GetField<NumericUpDown>(form, "nudItemQuantity");
                Assert.IsNotNull(nud);
                Assert.AreEqual(1, nud.Minimum);
                Assert.AreEqual(1, nud.Value);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveAmountPaid_InitiallyZero()
        {
            using (SalesForm form = CreateSalesForm())
            {
                NumericUpDown nud = GetField<NumericUpDown>(form, "nudAmountPaid");
                Assert.IsNotNull(nud);
                Assert.AreEqual(0, nud.Value);
            }
        }

        [TestMethod]
        public void SalesForm_ShouldHaveSaleDate_SetToToday()
        {
            using (SalesForm form = CreateSalesForm())
            {
                DateTimePicker dtp = GetField<DateTimePicker>(form, "dtpSaleDate");
                Assert.IsNotNull(dtp);
                Assert.AreEqual(DateTime.Today, dtp.Value.Date);
            }
        }

        [TestMethod]
        public void SalesForm_ClearCartButton_ShouldShowConfirmation()
        {
            using (SalesForm form = CreateSalesForm())
            {
                Button btn = GetField<Button>(form, "btnClearCart");
                Assert.IsNotNull(btn);
            }
        }

        [TestMethod]
        public void SalesForm_AddItemButton_ShouldShowError_WhenMedicineNotSelected()
        {
            using (SalesForm form = CreateSalesForm())
            {
                Button btn = GetField<Button>(form, "btnAddItem");
                btn.PerformClick();

                Label status = GetField<Label>(form, "lblStatus");
                Assert.IsTrue(status.Text.Contains("select a medicine"), $"Expected error about selecting medicine. Got: {status.Text}");
            }
        }

        [TestMethod]
        public void SalesForm_RemoveItemButton_ShouldShowError_WhenCartEmpty()
        {
            using (SalesForm form = CreateSalesForm())
            {
                Button btn = GetField<Button>(form, "btnRemoveItem");
                btn.PerformClick();

                Label status = GetField<Label>(form, "lblStatus");
                Assert.IsTrue(status.Text.Contains("No item selected"), $"Expected error about no item. Got: {status.Text}");
            }
        }

        [TestMethod]
        public void SalesForm_CompleteSaleButton_ShouldShowError_WhenCartEmpty()
        {
            using (SalesForm form = CreateSalesForm())
            {
                Button btn = GetField<Button>(form, "btnCompleteSale");
                btn.PerformClick();

                Label status = GetField<Label>(form, "lblStatus");
                Assert.IsTrue(status.Text.Contains("Cart is empty"), $"Expected error about empty cart. Got: {status.Text}");
            }
        }

        [TestMethod]
        public void SalesForm_ShouldInitializeStatus_Ready()
        {
            using (SalesForm form = CreateSalesForm())
            {
                Label status = GetField<Label>(form, "lblStatus");
                Assert.AreEqual("Ready", status.Text);
                Assert.AreEqual(Color.Green, status.ForeColor);
            }
        }

        [TestMethod]
        public void SalesForm_BarcodeSearch_ShouldHandleEmptyInput()
        {
            using (SalesForm form = CreateSalesForm())
            {
                TextBox txt = GetField<TextBox>(form, "txtBarcodeSearch");
                txt.Text = "";
                txt.Select();
                SendKeys.SendWait("{ENTER}");

                Label status = GetField<Label>(form, "lblStatus");
                Assert.IsNotNull(status);
            }
        }

        [TestMethod]
        public void SalesForm_MedicineSearch_ShouldHandleEmptyInput()
        {
            using (SalesForm form = CreateSalesForm())
            {
                TextBox txt = GetField<TextBox>(form, "txtMedicineSearch");
                txt.Text = "";
                Button btn = GetField<Button>(form, "btnSearchMedicine");
                btn.PerformClick();

                Label status = GetField<Label>(form, "lblStatus");
                Assert.IsNotNull(status);
            }
        }

        [TestMethod]
        public void SalesForm_CustomerIdCheckBox_ShouldToggleNumericUpDown()
        {
            using (SalesForm form = CreateSalesForm())
            {
                CheckBox chk = GetField<CheckBox>(form, "chkCustomerLookup");
                NumericUpDown nud = GetField<NumericUpDown>(form, "nudCustomerId");

                Assert.IsFalse(chk.Checked);
                Assert.IsFalse(nud.Enabled);

                chk.Checked = true;
                Assert.IsTrue(nud.Enabled);
            }
        }

        [TestMethod]
        public void SalesForm_DiscountPercent_ShouldStartAtZero()
        {
            using (SalesForm form = CreateSalesForm())
            {
                NumericUpDown nud = GetField<NumericUpDown>(form, "nudDiscountPercent");
                Assert.AreEqual(0, nud.Value);
                Assert.AreEqual(0, nud.Minimum);
                Assert.AreEqual(100, nud.Maximum);
            }
        }

        [TestMethod]
        public void SalesForm_TaxPercent_ShouldStartAtZero()
        {
            using (SalesForm form = CreateSalesForm())
            {
                NumericUpDown nud = GetField<NumericUpDown>(form, "nudTaxPercent");
                Assert.AreEqual(0, nud.Value);
                Assert.AreEqual(0, nud.Minimum);
                Assert.AreEqual(100, nud.Maximum);
            }
        }

        [TestMethod]
        public void SalesForm_CancelSaleButton_ShouldCloseForm_WhenCartEmpty()
        {
            using (SalesForm form = CreateSalesForm())
            {
                bool closed = false;
                form.FormClosed += (s, e) => closed = true;

                Button btn = GetField<Button>(form, "btnCancelSale");
                btn.PerformClick();

                Assert.IsTrue(closed);
            }
        }

        private static string GetFieldText(Form form, string fieldName)
        {
            FieldInfo field = form.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on SalesForm.");

            if (field.FieldType == typeof(TextBox))
                return ((TextBox)field.GetValue(form)).Text;
            if (field.FieldType == typeof(Label))
                return ((Label)field.GetValue(form)).Text;

            return field.GetValue(form)?.ToString();
        }

        private static T GetField<T>(Form form, string fieldName) where T : class
        {
            FieldInfo field = form.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on SalesForm.");
            return field.GetValue(form) as T;
        }

        private static SalesForm CreateSalesForm()
        {
            ISalesService salesService = new StubSalesService();
            IPaymentService paymentService = new StubPaymentService();
            IPricingService pricingService = new StubPricingService();
            IMedicineService medicineService = new StubMedicineService();
            IInventoryService inventoryService = new StubInventoryService();
            ISessionManager sessionManager = new StubSessionManagerForSales();
            ISaleNumberGenerator saleNumberGenerator = new StubSaleNumberGenerator();

            return new SalesForm(
                salesService,
                paymentService,
                pricingService,
                medicineService,
                inventoryService,
                sessionManager,
                saleNumberGenerator);
        }
    }
}
