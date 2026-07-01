using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.UI.Forms
{
    public class MedicineCategoryForm : Form
    {
        private readonly IMedicineCategoryService _categoryService;

        private TextBox txtName;
        private TextBox txtDescription;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRefresh;
        private DataGridView dgvCategories;
        private Label lblStatus;

        public MedicineCategoryForm(IMedicineCategoryService categoryService)
        {
            _categoryService = categoryService;
            InitializeComponents();
            LoadCategories();
        }

        private void InitializeComponents()
        {
            Text = "SmartMed - Medicine Categories";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(640, 480);
            ShowIcon = false;

            Label lblName = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(16, 16),
                Text = "Name:"
            };

            txtName = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(100, 13),
                Width = 200
            };

            Label lblDescription = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(16, 52),
                Text = "Description:"
            };

            txtDescription = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(100, 49),
                Width = 350
            };

            btnAdd = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(16, 90),
                Width = 90,
                Height = 28,
                Text = "Add"
            };
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(114, 90),
                Width = 90,
                Height = 28,
                Text = "Update"
            };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(212, 90),
                Width = 90,
                Height = 28,
                Text = "Delete"
            };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(310, 90),
                Width = 90,
                Height = 28,
                Text = "Refresh"
            };
            btnRefresh.Click += (s, e) => LoadCategories();

            dgvCategories = new DataGridView
            {
                Location = new Point(16, 130),
                Width = 600,
                Height = 280,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvCategories.SelectionChanged += DgvCategories_SelectionChanged;

            lblStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Green,
                Location = new Point(16, 420),
                Width = 600,
                Text = "Ready"
            };

            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblDescription);
            Controls.Add(txtDescription);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(btnRefresh);
            Controls.Add(dgvCategories);
            Controls.Add(lblStatus);
        }

        private void LoadCategories()
        {
            try
            {
                OperationResult<List<MedicineCategory>> result = _categoryService.GetAllCategories();
                if (result.IsSuccess)
                {
                    dgvCategories.DataSource = null;
                    dgvCategories.DataSource = result.Data;

                    if (dgvCategories.Columns.Contains("Id"))
                        dgvCategories.Columns["Id"].Width = 40;
                    if (dgvCategories.Columns.Contains("Name"))
                        dgvCategories.Columns["Name"].Width = 180;
                    if (dgvCategories.Columns.Contains("Description"))
                        dgvCategories.Columns["Description"].Width = 280;
                    if (dgvCategories.Columns.Contains("IsActive"))
                        dgvCategories.Columns["IsActive"].Visible = false;
                    if (dgvCategories.Columns.Contains("CreatedDate"))
                        dgvCategories.Columns["CreatedDate"].Visible = false;
                    if (dgvCategories.Columns.Contains("UpdatedDate"))
                        dgvCategories.Columns["UpdatedDate"].Visible = false;

                    SetStatus($"Loaded {result.Data.Count} categories.", Color.Green);
                }
                else
                {
                    SetStatus(result.Message, Color.Red);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error loading categories: {ex.Message}", Color.Red);
            }
        }

        private void DgvCategories_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count > 0)
            {
                MedicineCategory category = dgvCategories.SelectedRows[0].DataBoundItem as MedicineCategory;
                if (category != null)
                {
                    txtName.Text = category.Name;
                    txtDescription.Text = category.Description ?? "";
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            var category = new MedicineCategory
            {
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            OperationResult<int> result = _categoryService.AddCategory(category);

            if (result.IsSuccess)
            {
                ClearFields();
                LoadCategories();
                SetStatus("Category added successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count == 0)
            {
                SetStatus("Please select a category to update.", Color.Red);
                return;
            }

            MedicineCategory existing = dgvCategories.SelectedRows[0].DataBoundItem as MedicineCategory;
            if (existing == null) return;

            lblStatus.Text = "";
            existing.Name = txtName.Text.Trim();
            existing.Description = txtDescription.Text.Trim();

            OperationResult result = _categoryService.UpdateCategory(existing);

            if (result.IsSuccess)
            {
                LoadCategories();
                SetStatus("Category updated successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count == 0)
            {
                SetStatus("Please select a category to delete.", Color.Red);
                return;
            }

            MedicineCategory category = dgvCategories.SelectedRows[0].DataBoundItem as MedicineCategory;
            if (category == null) return;

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to delete category '{category.Name}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            OperationResult result = _categoryService.DeleteCategory(category.Id);

            if (result.IsSuccess)
            {
                ClearFields();
                LoadCategories();
                SetStatus("Category deleted successfully.", Color.Green);
            }
            else
            {
                SetStatus(result.Message, Color.Red);
            }
        }

        private void ClearFields()
        {
            txtName.Text = "";
            txtDescription.Text = "";
        }

        private void SetStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
