using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Results;
using SmartMed.Models.Session;

namespace SmartMed.UI.Forms
{
    public class LoginForm : Form
    {
        private readonly IAuthenticationService _authService;

        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;
        private LinkLabel linkCustomerLookup;
        private Label lblStatus;
        private int _failedAttemptCount;
        private Timer _cooldownTimer;
        private int _cooldownRemaining;

        public LoginForm(IAuthenticationService authService)
        {
            _authService = authService;
            _failedAttemptCount = 0;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "SmartMed Login";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(380, 260);
            ShowIcon = false;

            Label lblUsername = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(30, 30),
                Text = "Username:"
            };

            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(120, 27),
                Width = 220
            };

            Label lblPassword = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(30, 70),
                Text = "Password:"
            };

            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(120, 67),
                Width = 220,
                UseSystemPasswordChar = true
            };

            lblStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Red,
                Location = new Point(30, 105),
                Width = 310,
                MaximumSize = new Size(310, 40),
                Text = ""
            };

            btnLogin = new Button
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(120, 155),
                Width = 100,
                Height = 30,
                Text = "Login"
            };
            btnLogin.Click += BtnLogin_Click;

            btnCancel = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(230, 155),
                Width = 100,
                Height = 30,
                Text = "Cancel"
            };
            btnCancel.Click += (s, e) => Application.Exit();

            linkCustomerLookup = new LinkLabel
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(120, 200),
                Text = "Login as Customer?"
            };
            linkCustomerLookup.LinkClicked += LinkCustomerLookup_LinkClicked;

            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(lblStatus);
            Controls.Add(btnLogin);
            Controls.Add(btnCancel);
            Controls.Add(linkCustomerLookup);

            txtUsername.TextChanged += (s, e) => UpdateLoginButtonState();
            txtPassword.TextChanged += (s, e) => UpdateLoginButtonState();

            UpdateLoginButtonState();
        }

        private void UpdateLoginButtonState()
        {
            btnLogin.Enabled = txtUsername.Text.Trim().Length > 0 && txtPassword.Text.Length > 0;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            btnLogin.Enabled = false;

            OperationResult<SessionContext> result = _authService.LoginAdmin(
                txtUsername.Text.Trim(),
                txtPassword.Text);

            if (result.IsSuccess)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _failedAttemptCount++;
                lblStatus.Text = result.Message;

                if (_failedAttemptCount >= 3)
                {
                    StartCooldown();
                }
                else
                {
                    btnLogin.Enabled = true;
                }
            }
        }

        private void StartCooldown()
        {
            _cooldownRemaining = 30;
            btnLogin.Enabled = false;
            lblStatus.Text = $"Too many failed attempts. Please wait {_cooldownRemaining} second(s).";

            _cooldownTimer = new Timer
            {
                Interval = 1000
            };
            _cooldownTimer.Tick += (s, e) =>
            {
                _cooldownRemaining--;
                if (_cooldownRemaining <= 0)
                {
                    _cooldownTimer.Stop();
                    _cooldownTimer.Dispose();
                    _cooldownTimer = null;
                    _failedAttemptCount = 0;
                    lblStatus.Text = "";
                    UpdateLoginButtonState();
                }
                else
                {
                    lblStatus.Text = $"Too many failed attempts. Please wait {_cooldownRemaining} second(s).";
                }
            };
            _cooldownTimer.Start();
        }

        private void LinkCustomerLookup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (CustomerLookupForm customerForm = new CustomerLookupForm(_authService))
            {
                if (customerForm.ShowDialog(this) == DialogResult.OK)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }
    }
}
