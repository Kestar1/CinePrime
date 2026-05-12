using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CinePrime.BLL.Models;
using CinePrime.Common;
using CinePrime.DAL.Entities;

namespace CinePrime.UI.Forms
{
    public class UsersForm : Form
    {
        private readonly ServiceRegistry _services;
        private readonly DataGridView _usersGrid;
        private readonly TextBox _fullNameTextBox;
        private readonly TextBox _emailTextBox;
        private readonly TextBox _passwordTextBox;
        private readonly TextBox _searchTextBox;
        private readonly ComboBox _roleComboBox;
        private readonly ComboBox _statusComboBox;
        private int _selectedUserId;

        public UsersForm(ServiceRegistry services)
        {
            _services = services;
            Text = "Users Management (Admin)";
            Width = 1120;
            Height = 720;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new System.Drawing.Size(980, 640);
            Padding = new Padding(22);
            ThemeManager.Bind(this);

            if (!ApplicationSession.IsAdmin)
            {
                Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "Acces interzis: doar admin.",
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                });
                return;
            }

            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 1
            };

            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "card",
                RowCount = 4,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));

            var toolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(22, 12, 22, 8) };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            toolbar.Controls.Add(new Label { Text = "Users", Dock = DockStyle.Fill, Font = ThemeManager.SubtitleFont, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 0);
            _searchTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Search user..." };
            _searchTextBox.TextChanged += (_, __) => RefreshGrid();
            toolbar.Controls.Add(_searchTextBox, 1, 0);
            root.Controls.Add(toolbar, 0, 0);

            _usersGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(22, 0, 22, 12)
            };
            _usersGrid.SelectionChanged += (_, __) => LoadSelectedUser();
            root.Controls.Add(_usersGrid, 0, 1);

            var formPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 4,
                Padding = new Padding(22, 8, 22, 8)
            };
            for (var i = 0; i < 6; i++)
            {
                formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66f));
            }
            for (var i = 0; i < 4; i++)
            {
                formPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            }

            formPanel.Controls.Add(new Label { Text = "Nume", Dock = DockStyle.Fill, Font = ThemeManager.CaptionFont }, 0, 0);
            _fullNameTextBox = new TextBox { Dock = DockStyle.Fill };
            formPanel.SetColumnSpan(_fullNameTextBox, 2);
            formPanel.Controls.Add(_fullNameTextBox, 1, 0);

            formPanel.Controls.Add(new Label { Text = "Email", Dock = DockStyle.Fill, Font = ThemeManager.CaptionFont }, 3, 0);
            _emailTextBox = new TextBox { Dock = DockStyle.Fill };
            formPanel.SetColumnSpan(_emailTextBox, 2);
            formPanel.Controls.Add(_emailTextBox, 4, 0);

            formPanel.Controls.Add(new Label { Text = "Parola", Dock = DockStyle.Fill, Font = ThemeManager.CaptionFont }, 0, 1);
            _passwordTextBox = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
            formPanel.SetColumnSpan(_passwordTextBox, 2);
            formPanel.Controls.Add(_passwordTextBox, 1, 1);

            formPanel.Controls.Add(new Label { Text = "Rol", Dock = DockStyle.Fill, Font = ThemeManager.CaptionFont }, 3, 1);
            _roleComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _roleComboBox.Items.Add(Constants.RoleOperator);
            _roleComboBox.Items.Add(Constants.RoleAdmin);
            _roleComboBox.SelectedIndex = 0;
            formPanel.Controls.Add(_roleComboBox, 4, 1);

            formPanel.Controls.Add(new Label { Text = "Status", Dock = DockStyle.Fill, Font = ThemeManager.CaptionFont }, 0, 2);
            _statusComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _statusComboBox.Items.Add(Constants.StatusActive);
            _statusComboBox.Items.Add(Constants.StatusInactive);
            _statusComboBox.SelectedIndex = 0;
            formPanel.Controls.Add(_statusComboBox, 1, 2);

            root.Controls.Add(formPanel, 0, 2);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(22, 10, 22, 14)
            };

            var addButton = CreateActionButton("Adauga utilizator");
            addButton.Click += OnAddUserClick;
            actions.Controls.Add(addButton);

            var updateButton = CreateActionButton("Actualizeaza");
            updateButton.Click += OnUpdateUserClick;
            actions.Controls.Add(updateButton);

            var resetButton = CreateActionButton("Reset parola");
            resetButton.Click += OnResetPasswordClick;
            actions.Controls.Add(resetButton);

            var deleteButton = CreateActionButton("Dezactiveaza");
            deleteButton.Click += OnDeleteUserClick;
            actions.Controls.Add(deleteButton);

            root.Controls.Add(actions, 0, 3);
            outer.Controls.Add(root, 0, 0);
            Controls.Add(outer);

            RefreshGrid();
        }

        private static Button CreateActionButton(string text)
        {
            return new PremiumButton
            {
                Text = text,
                Width = 170,
                Height = 44,
                Margin = new Padding(10, 0, 0, 0),
                Font = ThemeManager.ButtonFont,
                Variant = text == "Dezactiveaza" ? PremiumButtonVariant.Secondary : PremiumButtonVariant.Primary
            };
        }

        private void OnAddUserClick(object sender, EventArgs e)
        {
            var request = new RegisterUserRequest
            {
                FullName = _fullNameTextBox.Text,
                Email = _emailTextBox.Text,
                Password = _passwordTextBox.Text,
                Role = _roleComboBox.SelectedItem?.ToString() ?? Constants.RoleOperator
            };

            var result = _services.AuthService.Register(request);
            MessageBox.Show(result.Message, result.Success ? "Succes" : "Eroare");
            if (!result.Success)
            {
                return;
            }

            _fullNameTextBox.Text = string.Empty;
            _emailTextBox.Text = string.Empty;
            _passwordTextBox.Text = string.Empty;
            _roleComboBox.SelectedIndex = 0;
            RefreshGrid();
        }

        private void OnUpdateUserClick(object sender, EventArgs e)
        {
            if (_selectedUserId == 0)
            {
                MessageBox.Show("Selecteaza un utilizator.", "Info");
                return;
            }

            var result = _services.UserService.UpdateUser(
                _selectedUserId,
                _fullNameTextBox.Text,
                _emailTextBox.Text,
                _roleComboBox.SelectedItem?.ToString() ?? Constants.RoleOperator,
                _statusComboBox.SelectedItem?.ToString() ?? Constants.StatusActive);
            MessageBox.Show(result.Message, result.Success ? "Succes" : "Eroare");
            RefreshGrid();
        }

        private void OnResetPasswordClick(object sender, EventArgs e)
        {
            if (_selectedUserId == 0)
            {
                MessageBox.Show("Selecteaza un utilizator.", "Info");
                return;
            }

            var result = _services.UserService.ResetPassword(_selectedUserId, _passwordTextBox.Text);
            MessageBox.Show(result.Message, result.Success ? "Succes" : "Eroare");
            if (result.Success)
            {
                _passwordTextBox.Text = string.Empty;
            }
        }

        private void OnDeleteUserClick(object sender, EventArgs e)
        {
            if (_selectedUserId == 0)
            {
                MessageBox.Show("Selecteaza un utilizator.", "Info");
                return;
            }

            var confirm = MessageBox.Show("Sigur vrei sa dezactivezi utilizatorul selectat?", "Confirmare", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            var result = _services.UserService.DeleteUser(_selectedUserId);
            MessageBox.Show(result.Message, result.Success ? "Succes" : "Eroare");
            RefreshGrid();
        }

        private void LoadSelectedUser()
        {
            if (_usersGrid.CurrentRow?.DataBoundItem is not User user)
            {
                return;
            }

            _selectedUserId = user.Id;
            _fullNameTextBox.Text = user.FullName;
            _emailTextBox.Text = user.Email;
            _passwordTextBox.Text = string.Empty;
            _roleComboBox.SelectedItem = user.Role;
            _statusComboBox.SelectedItem = user.Status;
        }

        private void RefreshGrid()
        {
            List<User> users = _services.UserService.GetUsers();
            var query = _searchTextBox?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(query))
            {
                users = users.Where(u =>
                    u.FullName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    u.Email.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    u.Role.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    u.Status.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            _usersGrid.DataSource = null;
            _usersGrid.DataSource = users;
            if (_usersGrid.Columns["PasswordHash"] != null)
            {
                _usersGrid.Columns["PasswordHash"].Visible = false;
            }
        }
    }
}
