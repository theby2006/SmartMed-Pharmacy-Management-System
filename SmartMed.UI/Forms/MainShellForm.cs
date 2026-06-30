using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Diagnostics;
using SmartMed.Models.Enums;
using SmartMed.Models.Session;

namespace SmartMed.UI.Forms
{
    public class MainShellForm : Form
    {
        private readonly ISessionManager _sessionManager;
        private readonly IAuthenticationService _authService;
        private ToolStripMenuItem _administrationMenu;
        private ToolStripMenuItem _prescriptionsMenu;
        private ToolStripMenuItem _salesMenu;
        private ToolStripMenuItem _reportsMenu;
        private ToolStripStatusLabel _userStatusLabel;

        public MainShellForm(ApplicationStartupContext startupContext, ISessionManager sessionManager, IAuthenticationService authService)
        {
            _sessionManager = sessionManager;
            _authService = authService;

            Text = startupContext.ApplicationName;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(760, 420);
            Width = 900;
            Height = 540;

            MenuStrip menuStrip = BuildMenuStrip();
            Controls.Add(menuStrip);

            StatusStrip statusStrip = BuildStatusStrip();
            Controls.Add(statusStrip);

            var headerLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(24, 32),
                Text = startupContext.ApplicationName
            };

            SessionContext session = _sessionManager.CurrentSession;
            string roleLabel = session != null ? "Logged in as: " + session.DisplayName + " (" + session.Role + ")" : "";

            var summaryLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Location = new Point(26, 80),
                Text = roleLabel
            };

            var detailsTextBox = new TextBox
            {
                Location = new Point(30, 120),
                Width = 820,
                Height = 340,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10F),
                Text = BuildDiagnosticsText(startupContext)
            };

            Controls.Add(headerLabel);
            Controls.Add(summaryLabel);
            Controls.Add(detailsTextBox);

            FormClosing += MainShellForm_FormClosing;

            UpdateMenuVisibility();
        }

        private MenuStrip BuildMenuStrip()
        {
            MenuStrip menuStrip = new MenuStrip();

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem logoutItem = new ToolStripMenuItem("Logout");
            logoutItem.Click += Logout_Click;
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => Application.Exit();

            fileMenu.DropDownItems.Add(logoutItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitItem);
            menuStrip.Items.Add(fileMenu);

            _administrationMenu = new ToolStripMenuItem("Administration");
            _administrationMenu.DropDownItems.Add(new ToolStripMenuItem("User Management", null, (s, e) => { }));
            _administrationMenu.DropDownItems.Add(new ToolStripMenuItem("Audit Log", null, (s, e) => { }));
            menuStrip.Items.Add(_administrationMenu);

            _prescriptionsMenu = new ToolStripMenuItem("Prescriptions");
            _prescriptionsMenu.DropDownItems.Add(new ToolStripMenuItem("New Prescription", null, (s, e) => { }));
            _prescriptionsMenu.DropDownItems.Add(new ToolStripMenuItem("View Prescriptions", null, (s, e) => { }));
            menuStrip.Items.Add(_prescriptionsMenu);

            _salesMenu = new ToolStripMenuItem("Sales");
            _salesMenu.DropDownItems.Add(new ToolStripMenuItem("New Sale", null, (s, e) => { }));
            _salesMenu.DropDownItems.Add(new ToolStripMenuItem("Sales History", null, (s, e) => { }));
            menuStrip.Items.Add(_salesMenu);

            _reportsMenu = new ToolStripMenuItem("Reports");
            _reportsMenu.DropDownItems.Add(new ToolStripMenuItem("Generate Report", null, (s, e) => { }));
            menuStrip.Items.Add(_reportsMenu);

            return menuStrip;
        }

        private StatusStrip BuildStatusStrip()
        {
            StatusStrip statusStrip = new StatusStrip();
            SessionContext session = _sessionManager.CurrentSession;

            string statusText = session != null
                ? session.DisplayName + " (" + session.Role + ")"
                : "Not authenticated";

            _userStatusLabel = new ToolStripStatusLabel(statusText);
            statusStrip.Items.Add(_userStatusLabel);

            return statusStrip;
        }

        private void UpdateMenuVisibility()
        {
            SessionContext session = _sessionManager.CurrentSession;
            bool isAdmin = session != null && session.Role == RoleType.Administrator;
            bool isPharmacist = session != null && session.Role == RoleType.Pharmacist;
            bool isCashier = session != null && session.Role == RoleType.Cashier;

            _administrationMenu.Visible = isAdmin;
            _prescriptionsMenu.Visible = isAdmin || isPharmacist;
            _salesMenu.Visible = isAdmin || isCashier;
            _reportsMenu.Visible = isAdmin;
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            _authService.Logout();
            Close();
        }

        private void MainShellForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_authService.IsAuthenticated)
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to exit?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private static string BuildDiagnosticsText(ApplicationStartupContext startupContext)
        {
            return
                "Foundation Diagnostics" + System.Environment.NewLine +
                "----------------------" + System.Environment.NewLine +
                "Data Provider: " + startupContext.DataProviderName + System.Environment.NewLine +
                "Prescription Upload Root: " + startupContext.PrescriptionUploadRootPath + System.Environment.NewLine +
                "Report Export Root: " + startupContext.ReportExportRootPath + System.Environment.NewLine +
                "Default Low Stock Threshold: " + startupContext.DefaultLowStockThreshold + System.Environment.NewLine +
                "Near Expiry Threshold Days: " + startupContext.NearExpiryThresholdDays + System.Environment.NewLine +
                "Session Timeout Minutes: " + startupContext.SessionTimeoutMinutes;
        }
    }
}
