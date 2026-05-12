using System;
using System.Drawing;
using System.Windows.Forms;
using CinePrime.BLL.Models;

namespace CinePrime.UI.Forms
{
    public class RegisterForm : Form
    {
        private readonly ServiceRegistry _services;
        private readonly TextBox _fullNameTextBox;
        private readonly TextBox _emailTextBox;
        private readonly TextBox _passwordTextBox;
        private readonly TextBox _confirmPasswordTextBox;

        public RegisterForm(ServiceRegistry services)
        {
            _services = services;
            Text = "CinePrime - Register";
            Width = 1040;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 640);
            Padding = new Padding(0);

            var background = new CinematicBackgroundPanel { Dock = DockStyle.Fill };
            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                BackColor = Color.Transparent,
                RowCount = 3,
                ColumnCount = 3
            };
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 540));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 820));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var card = new GlassPanel
            {
                Dock = DockStyle.Fill,
                Radius = 34,
                FillAlpha = 172,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(18)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

            var brandPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                Padding = new Padding(28, 34, 28, 34),
                RowCount = 6,
                ColumnCount = 1
            };
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            brandPanel.Controls.Add(new Label
            {
                Text = "CinePrime",
                Tag = "accent-text",
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.TitleFont.FontFamily, 34, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomLeft
            }, 0, 0);
            brandPanel.Controls.Add(new Label
            {
                Text = "Creeaza un cont nou pentru operator",
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.BodyFont.FontFamily, 13, FontStyle.Regular),
                TextAlign = ContentAlignment.TopLeft
            }, 0, 1);
            brandPanel.Controls.Add(new Label
            {
                Text = "Rol implicit: operator",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 2);
            brandPanel.Controls.Add(CreateRegisterStat("Validare date", "Email unic si parola minima 6 caractere"), 0, 3);
            brandPanel.Controls.Add(CreateRegisterStat("Acces rapid", "Dupa creare poti reveni la autentificare"), 0, 4);

            var form = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                Padding = new Padding(34, 28, 34, 26),
                RowCount = 10,
                ColumnCount = 1
            };
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            form.Controls.Add(new Label
            {
                Text = "Inregistrare",
                Dock = DockStyle.Fill,
                Font = ThemeManager.SubtitleFont,
                TextAlign = ContentAlignment.BottomLeft
            }, 0, 0);
            form.Controls.Add(new Label
            {
                Text = "Completeaza datele pentru contul nou",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.TopLeft
            }, 0, 1);
            form.Controls.Add(new Label { Text = string.Empty }, 0, 2);

            _fullNameTextBox = new TextBox { PlaceholderText = "Nume complet", Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
            _emailTextBox = new TextBox { PlaceholderText = "Email", Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
            _passwordTextBox = new TextBox { PlaceholderText = "Parola", UseSystemPasswordChar = true, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
            _confirmPasswordTextBox = new TextBox { PlaceholderText = "Confirma parola", UseSystemPasswordChar = true, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
            form.Controls.Add(_fullNameTextBox, 0, 3);
            form.Controls.Add(_emailTextBox, 0, 4);
            form.Controls.Add(_passwordTextBox, 0, 5);
            form.Controls.Add(_confirmPasswordTextBox, 0, 6);
            form.Controls.Add(new Label
            {
                Text = "Conturile create aici primesc rolul operator.",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 7);

            var buttons = new TableLayoutPanel { Dock = DockStyle.Fill, Tag = "transparent", ColumnCount = 2, RowCount = 1 };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            var createButton = new PremiumButton { Text = "Creeaza cont", Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4) };
            createButton.Click += OnCreateAccountClick;
            var backButton = new PremiumButton { Text = "Inapoi la login", Variant = PremiumButtonVariant.Secondary, Dock = DockStyle.Fill, Margin = new Padding(8, 4, 0, 4) };
            backButton.Click += (_, __) => Close();
            buttons.Controls.Add(createButton, 0, 0);
            buttons.Controls.Add(backButton, 1, 0);
            form.Controls.Add(buttons, 0, 8);

            split.Controls.Add(brandPanel, 0, 0);
            split.Controls.Add(form, 1, 0);
            card.Controls.Add(split);
            outer.Controls.Add(card, 1, 1);
            background.Controls.Add(outer);
            Controls.Add(background);
            ThemeManager.Bind(this);
        }

        private static Control CreateRegisterStat(string value, string label)
        {
            var panel = new GlassPanel
            {
                Dock = DockStyle.Fill,
                Radius = 18,
                FillAlpha = 96,
                Padding = new Padding(16, 8, 16, 8),
                Margin = new Padding(0, 6, 0, 6)
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Tag = "transparent", RowCount = 2, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            layout.Controls.Add(new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.ButtonFont.FontFamily, 15, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomLeft
            }, 0, 0);
            layout.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.TopLeft
            }, 0, 1);
            panel.Controls.Add(layout);
            return panel;
        }

        private void OnCreateAccountClick(object sender, EventArgs e)
        {
            if (!string.Equals(_passwordTextBox.Text, _confirmPasswordTextBox.Text, StringComparison.Ordinal))
            {
                MessageBox.Show("Parolele nu coincid.", "Eroare");
                return;
            }

            var result = _services.AuthService.Register(new RegisterUserRequest
            {
                FullName = _fullNameTextBox.Text,
                Email = _emailTextBox.Text,
                Password = _passwordTextBox.Text,
                Role = "operator"
            });

            MessageBox.Show(result.Message, result.Success ? "Succes" : "Eroare");
            if (result.Success)
            {
                Close();
            }
        }
    }
}
