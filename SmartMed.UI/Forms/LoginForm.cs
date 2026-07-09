using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Results;
using SmartMed.Models.Session;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    public class LoginForm : Form
    {
        private readonly IAuthenticationService _authService;
        private readonly ICustomerService _customerService;

        private ModernTextBox txtUsername;
        private ModernTextBox txtPassword;
        private RoundedButton btnLogin;
        private RoundedButton btnClose;
        private LinkLabel linkCustomerLookup;
        private int _failedAttemptCount;
        private Timer _cooldownTimer;
        private int _cooldownRemaining;
        private Point _dragStartPoint;
        private bool _isDragging;

        private const int BrandPanelWidth = 405;

        public LoginForm(IAuthenticationService authService, ICustomerService customerService)
        {
            _authService = authService;
            _customerService = customerService;
            _failedAttemptCount = 0;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "SmartMed Login";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(900, 560);
            ShowIcon = true;
            Icon = IconFactory.BuildAppIcon();
            BackColor = AppTheme.Surface;

            Panel brandPanel = BuildBrandPanel();
            Controls.Add(brandPanel);

            Panel formPanel = BuildFormPanel();
            Controls.Add(formPanel);

            btnClose = new RoundedButton
            {
                Variant = ButtonVariant.Ghost,
                Text = "✕",
                Font = new Font("Segoe UI", 10F),
                Size = new Size(32, 32),
                Location = new Point(ClientSize.Width - 32 - AppTheme.Space4, AppTheme.Space4)
            };
            btnClose.Click += (s, e) => Application.Exit();
            Controls.Add(btnClose);
            btnClose.BringToFront();

            MouseDown += Form_MouseDown;
            MouseMove += Form_MouseMove;
            MouseUp += Form_MouseUp;

            UpdateLoginButtonState();
        }

        private Panel BuildBrandPanel()
        {
            Panel panel = new Panel
            {
                Size = new Size(BrandPanelWidth, ClientSize.Height),
                Location = new Point(0, 0),
                BackColor = AppTheme.Sidebar
            };
            panel.Paint += BrandPanel_Paint;
            panel.MouseDown += Form_MouseDown;
            panel.MouseMove += Form_MouseMove;
            panel.MouseUp += Form_MouseUp;

            Label wordmark = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = AppTheme.TextOnPrimary,
                BackColor = Color.Transparent,
                Location = new Point(64, 220),
                Text = "SmartMed"
            };
            panel.Controls.Add(wordmark);

            Label tagline = new Label
            {
                AutoSize = true,
                Font = AppTheme.Body,
                ForeColor = AppTheme.SidebarText,
                BackColor = Color.Transparent,
                Location = new Point(64, 260),
                MaximumSize = new Size(BrandPanelWidth - 128, 0),
                Text = "Pharmacy Management, Simplified"
            };
            panel.Controls.Add(tagline);

            return panel;
        }

        private void BrandPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Decorative low-alpha ring watermark.
            using (Pen ringPen = new Pen(Color.FromArgb(18, 255, 255, 255), 24))
            {
                g.DrawEllipse(ringPen, new Rectangle(-120, 300, 420, 420));
            }
            using (Pen ringPen2 = new Pen(Color.FromArgb(10, 255, 255, 255), 40))
            {
                g.DrawEllipse(ringPen2, new Rectangle(120, -180, 380, 380));
            }

            // Pharmacy cross accent above the wordmark.
            int crossSize = 40;
            int crossThickness = 10;
            int cx = 64 + crossSize / 2;
            int cy = 150;
            using (SolidBrush accentBrush = new SolidBrush(AppTheme.Accent))
            {
                g.FillRectangle(accentBrush, cx - crossSize / 2, cy - crossThickness / 2, crossSize, crossThickness);
                g.FillRectangle(accentBrush, cx - crossThickness / 2, cy - crossSize / 2, crossThickness, crossSize);
            }
        }

        private Panel BuildFormPanel()
        {
            Panel panel = new Panel
            {
                Size = new Size(ClientSize.Width - BrandPanelWidth, ClientSize.Height),
                Location = new Point(BrandPanelWidth, 0),
                BackColor = AppTheme.Surface
            };

            int contentLeft = 72;
            int contentWidth = panel.Width - contentLeft * 2;
            int y = 140;

            Label title = new Label
            {
                AutoSize = true,
                Font = AppTheme.PageTitle,
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(contentLeft, y),
                Text = "Welcome back"
            };
            panel.Controls.Add(title);
            y += 36;

            Label subtitle = new Label
            {
                AutoSize = true,
                Font = AppTheme.Body,
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(contentLeft, y),
                Text = "Sign in to your account to continue"
            };
            panel.Controls.Add(subtitle);
            y += 48;

            txtUsername = new ModernTextBox
            {
                Location = new Point(contentLeft, y),
                Width = contentWidth,
                PlaceholderText = "Username",
                LeadingIcon = IconFactory.User
            };
            txtUsername.TextChanged += (s, e) => UpdateLoginButtonState();
            panel.Controls.Add(txtUsername);
            y += txtUsername.Height + AppTheme.Space3;

            txtPassword = new ModernTextBox
            {
                Location = new Point(contentLeft, y),
                Width = contentWidth,
                PlaceholderText = "Password",
                LeadingIcon = IconFactory.Lock,
                IsPasswordField = true
            };
            txtPassword.TextChanged += (s, e) => UpdateLoginButtonState();
            panel.Controls.Add(txtPassword);
            y += txtPassword.Height + AppTheme.Space4;

            btnLogin = new RoundedButton
            {
                Variant = ButtonVariant.Primary,
                Text = "Sign in",
                Location = new Point(contentLeft, y),
                Width = contentWidth,
                Height = AppTheme.ControlHeight
            };
            btnLogin.Click += BtnLogin_Click;
            panel.Controls.Add(btnLogin);
            y += btnLogin.Height + AppTheme.Space6;

            Panel divider = new Panel
            {
                BackColor = AppTheme.Border,
                Location = new Point(contentLeft, y),
                Width = contentWidth,
                Height = 1
            };
            panel.Controls.Add(divider);
            y += AppTheme.Space4;

            linkCustomerLookup = new LinkLabel
            {
                AutoSize = true,
                Font = AppTheme.Body,
                LinkColor = AppTheme.Accent,
                Text = "Continue as Customer →"
            };
            linkCustomerLookup.Location = new Point(contentLeft + (contentWidth - linkCustomerLookup.PreferredWidth) / 2, y);
            linkCustomerLookup.LinkClicked += LinkCustomerLookup_LinkClicked;
            panel.Controls.Add(linkCustomerLookup);

            return panel;
        }

        private void UpdateLoginButtonState()
        {
            if (_cooldownTimer != null && _cooldownTimer.Enabled)
                return;

            btnLogin.Enabled = txtUsername.Text.Trim().Length > 0 && txtPassword.Text.Length > 0;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            txtUsername.ClearError();
            txtPassword.ClearError();
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
                txtPassword.SetError(result.Message);

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
            btnLogin.Text = $"Please wait {_cooldownRemaining}s...";

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
                    btnLogin.Text = "Sign in";
                    txtPassword.ClearError();
                    UpdateLoginButtonState();
                }
                else
                {
                    btnLogin.Text = $"Please wait {_cooldownRemaining}s...";
                }
            };
            _cooldownTimer.Start();
        }

        private void LinkCustomerLookup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (CustomerLookupForm customerForm = new CustomerLookupForm(_authService, _customerService))
            {
                if (customerForm.ShowDialog(this) == DialogResult.OK)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            try
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
            catch (EntryPointNotFoundException)
            {
                // Non-Windows test/runtime environment: fall back to a no-op,
                // the window simply cannot be dragged there.
                _isDragging = true;
                _dragStartPoint = Cursor.Position;
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            Point current = Cursor.Position;
            Location = new Point(Location.X + (current.X - _dragStartPoint.X), Location.Y + (current.Y - _dragStartPoint.Y));
            _dragStartPoint = current;
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }
    }
}
