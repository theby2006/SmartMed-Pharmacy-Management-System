using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;
using SmartMed.Models.Session;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    /// <summary>
    /// Self-service customer account creation. On success, immediately signs
    /// the new customer in and closes with <see cref="DialogResult.OK"/> so
    /// the caller (<see cref="CustomerLookupForm"/>/<see cref="LoginForm"/>)
    /// can proceed straight into the customer shell.
    /// </summary>
    public class CustomerRegistrationForm : Form
    {
        private readonly ICustomerService _customerService;
        private readonly IAuthenticationService _authService;

        private ModernTextBox txtFullName;
        private ModernTextBox txtPhone;
        private ModernTextBox txtEmail;
        private ModernTextBox txtAddress;
        private ModernTextBox txtCity;
        private ModernTextBox txtPin;
        private ModernTextBox txtConfirmPin;
        private RoundedButton btnCreate;
        private Label lblFormError;

        public CustomerRegistrationForm(ICustomerService customerService, IAuthenticationService authService)
        {
            _customerService = customerService;
            _authService = authService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Create Customer Account";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(480, 640);
            BackColor = AppTheme.Background;
            Icon = IconFactory.BuildAppIcon();
            ShowIcon = true;

            CardPanel card = new CardPanel
            {
                Location = new Point(24, 24),
                Size = new Size(432, 592)
            };
            Controls.Add(card);

            int left = 24;
            int width = card.Width - 48;
            int y = 20;

            Label title = new Label
            {
                AutoSize = true,
                Font = AppTheme.SectionHeader,
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(left, y),
                Text = "Create your account"
            };
            card.Controls.Add(title);
            y += 40;

            txtFullName = AddField(card, "Full name", IconFactory.User, left, width, ref y);
            txtPhone = AddField(card, "Phone number", IconFactory.User, left, width, ref y);
            txtEmail = AddField(card, "Email (optional)", IconFactory.User, left, width, ref y);
            txtAddress = AddField(card, "Address (optional)", IconFactory.User, left, width, ref y);
            txtCity = AddField(card, "City (optional)", IconFactory.User, left, width, ref y);

            txtPin = AddField(card, "PIN (4-8 digits)", IconFactory.Lock, left, width, ref y);
            txtPin.IsPasswordField = true;

            txtConfirmPin = AddField(card, "Confirm PIN", IconFactory.Lock, left, width, ref y);
            txtConfirmPin.IsPasswordField = true;

            lblFormError = new Label
            {
                AutoSize = false,
                Font = AppTheme.Caption,
                ForeColor = AppTheme.Danger,
                Location = new Point(left, y),
                Size = new Size(width, 32),
                Visible = false
            };
            card.Controls.Add(lblFormError);
            y += 8;

            btnCreate = new RoundedButton
            {
                Variant = ButtonVariant.Primary,
                Text = "Create account",
                Location = new Point(left, y),
                Width = width,
                Height = AppTheme.ControlHeight
            };
            btnCreate.Click += BtnCreate_Click;
            card.Controls.Add(btnCreate);
        }

        private ModernTextBox AddField(Control parent, string placeholder, char icon, int left, int width, ref int y)
        {
            ModernTextBox field = new ModernTextBox
            {
                Location = new Point(left, y),
                Width = width,
                PlaceholderText = placeholder,
                LeadingIcon = icon
            };
            parent.Controls.Add(field);
            y += field.Height + AppTheme.Space2;
            return field;
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            lblFormError.Visible = false;
            txtFullName.ClearError();
            txtPhone.ClearError();
            txtEmail.ClearError();
            txtPin.ClearError();
            txtConfirmPin.ClearError();

            if (txtPin.Text != txtConfirmPin.Text)
            {
                txtConfirmPin.SetError("PINs do not match.");
                return;
            }

            Customer customer = new Customer
            {
                FullName = txtFullName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                City = string.IsNullOrWhiteSpace(txtCity.Text) ? null : txtCity.Text.Trim()
            };

            string pin = string.IsNullOrWhiteSpace(txtPin.Text) ? null : txtPin.Text.Trim();

            btnCreate.Enabled = false;
            OperationResult<int> result = _customerService.RegisterCustomer(customer, pin);

            if (!result.IsSuccess)
            {
                btnCreate.Enabled = true;
                lblFormError.Text = result.Message;
                lblFormError.Visible = true;
                return;
            }

            OperationResult<SessionContext> loginResult = _authService.LoginCustomer(customer.PhoneNumber, pin);
            btnCreate.Enabled = true;

            if (loginResult.IsSuccess)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblFormError.Text = "Account created. Please sign in with your new details.";
                lblFormError.ForeColor = AppTheme.Success;
                lblFormError.Visible = true;
            }
        }
    }
}
