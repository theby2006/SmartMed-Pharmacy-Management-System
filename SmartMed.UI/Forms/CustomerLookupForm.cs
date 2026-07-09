using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.Models.Results;
using SmartMed.Models.Session;

namespace SmartMed.UI.Forms
{
    public class CustomerLookupForm : Form
    {
        private readonly IAuthenticationService _authService;
        private readonly ICustomerService _customerService;

        private TextBox txtPhoneOrEmail;
        private TextBox txtPin;
        private Button btnLookup;
        private Button btnCancel;
        private Label lblStatus;
        private LinkLabel linkRegister;

        public CustomerLookupForm(IAuthenticationService authService, ICustomerService customerService)
        {
            _authService = authService;
            _customerService = customerService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "SmartMed Customer Lookup";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(380, 260);
            ShowIcon = false;

            Label lblPhoneOrEmail = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(30, 30),
                Text = "Phone or Email:"
            };

            txtPhoneOrEmail = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(150, 27),
                Width = 190
            };

            bool customerPinEnabled = false;
            try
            {
                customerPinEnabled = AppSettings.GetRequiredBool(ConfigKeys.CustomerPinEnabled);
            }
            catch
            {
                customerPinEnabled = false;
            }

            Label lblPin = null;

            if (customerPinEnabled)
            {
                lblPin = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F),
                    Location = new Point(30, 70),
                    Text = "PIN:"
                };

                txtPin = new TextBox
                {
                    Font = new Font("Segoe UI", 10F),
                    Location = new Point(150, 67),
                    Width = 190,
                    UseSystemPasswordChar = true
                };
            }

            lblStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Red,
                Location = new Point(30, 110),
                Width = 310,
                MaximumSize = new Size(310, 40),
                Text = ""
            };

            int buttonY = customerPinEnabled ? 160 : 150;

            btnLookup = new Button
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(150, buttonY),
                Width = 90,
                Height = 30,
                Text = "Lookup"
            };
            btnLookup.Click += BtnLookup_Click;

            btnCancel = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(250, buttonY),
                Width = 90,
                Height = 30,
                Text = "Cancel"
            };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            linkRegister = new LinkLabel
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(150, buttonY + 40),
                Text = "New here? Create an account"
            };
            linkRegister.LinkClicked += LinkRegister_LinkClicked;

            Controls.Add(lblPhoneOrEmail);
            Controls.Add(txtPhoneOrEmail);

            if (lblPin != null)
            {
                Controls.Add(lblPin);
            }

            if (txtPin != null)
            {
                Controls.Add(txtPin);
            }

            Controls.Add(lblStatus);
            Controls.Add(btnLookup);
            Controls.Add(btnCancel);
            Controls.Add(linkRegister);

            txtPhoneOrEmail.TextChanged += (s, e) => UpdateLookupButtonState();
            if (txtPin != null)
            {
                txtPin.TextChanged += (s, e) => UpdateLookupButtonState();
            }

            UpdateLookupButtonState();

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            };
        }

        private void UpdateLookupButtonState()
        {
            bool hasIdentifier = txtPhoneOrEmail.Text.Trim().Length > 0;
            if (txtPin != null)
            {
                btnLookup.Enabled = hasIdentifier && txtPin.Text.Length > 0;
            }
            else
            {
                btnLookup.Enabled = hasIdentifier;
            }
        }

        private void BtnLookup_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            btnLookup.Enabled = false;

            string pin = txtPin != null ? txtPin.Text : null;

            OperationResult<SessionContext> result = _authService.LoginCustomer(
                txtPhoneOrEmail.Text.Trim(),
                pin);

            if (result.IsSuccess)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblStatus.Text = result.Message;
                btnLookup.Enabled = true;
            }
        }

        private void LinkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (CustomerRegistrationForm registrationForm = new CustomerRegistrationForm(_customerService, _authService))
            {
                if (registrationForm.ShowDialog(this) == DialogResult.OK)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }
    }
}
