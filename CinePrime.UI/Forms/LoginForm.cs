using System;
using System.Drawing;
using System.Windows.Forms;
using CinePrime.BLL.Models;

namespace CinePrime.UI.Forms
{
    public class LoginForm : Form
    {
        private readonly ServiceRegistry _services;
        private readonly TextBox _emailTextBox;
        private readonly TextBox _passwordTextBox;
        private readonly PremiumButton _loginButton;
        private readonly PremiumButton _registerButton;

        public LoginForm(ServiceRegistry services)
        {
            _services = services;
            Text = "CinePrime - Login";
            Width = 1040;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 640);
            Padding = new Padding(0);

            var background = new CinematicBackgroundPanel
            {
                Dock = DockStyle.Fill
            };

            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Tag = "transparent",
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
                Padding = new Padding(0),
                Margin = new Padding(0),
                Radius = 34,
                FillAlpha = 172
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
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
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
                Text = "Premium cinema management",
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.BodyFont.FontFamily, 13, FontStyle.Regular),
                TextAlign = ContentAlignment.TopLeft
            }, 0, 1);
            brandPanel.Controls.Add(new Label
            {
                Text = "Live dashboard | Rezervari | Vanzari",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 2);
            brandPanel.Controls.Add(CreateLoginStat("2 sali active", "Programari si locuri controlate rapid"), 0, 3);
            brandPanel.Controls.Add(CreateLoginStat("460 MDL azi", "Demo cu date gata pentru testare"), 0, 4);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                Padding = new Padding(34, 34, 34, 28),
                RowCount = 9,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var title = new Label { Text = "Autentificare", Dock = DockStyle.Fill, Font = ThemeManager.SubtitleFont, TextAlign = ContentAlignment.BottomLeft };
            layout.Controls.Add(title, 0, 0);

            var subtitle = new Label { Text = "Intra in consola de administrare", Dock = DockStyle.Fill, Font = ThemeManager.CaptionFont, TextAlign = ContentAlignment.TopLeft };
            layout.Controls.Add(subtitle, 0, 1);

            var spacer = new Label { Text = string.Empty };
            layout.Controls.Add(spacer, 0, 2);

            _emailTextBox = new TextBox { PlaceholderText = "Email", Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
            layout.Controls.Add(_emailTextBox, 0, 3);

            _passwordTextBox = new TextBox { PlaceholderText = "Parola", UseSystemPasswordChar = true, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
            layout.Controls.Add(_passwordTextBox, 0, 4);

            var info = new Label
            {
                Text = "Demo: admin@cineprime.md / admin123 sau operator@cineprime.md / operator123",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(info, 0, 5);

            var buttons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Tag = "transparent", Padding = new Padding(0) };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _loginButton = new PremiumButton { Text = "Login", Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4) };
            _loginButton.Click += OnLoginClick;
            buttons.Controls.Add(_loginButton, 0, 0);

            _registerButton = new PremiumButton { Text = "Register", Variant = PremiumButtonVariant.Primary, Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4) };
            _registerButton.Click += OnRegisterClick;
            buttons.Controls.Add(_registerButton, 1, 0);
            layout.Controls.Add(buttons, 0, 6);

            split.Controls.Add(brandPanel, 0, 0);
            split.Controls.Add(layout, 1, 0);
            card.Controls.Add(split);
            outer.Controls.Add(card, 1, 1);
            background.Controls.Add(outer);
            Controls.Add(background);
            ThemeManager.Bind(this);
        }

        private static Control CreateLoginStat(string value, string label)
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

        private void OnLoginClick(object sender, EventArgs e)
        {
            var sessionUser = _services.AuthService.Login(_emailTextBox.Text, _passwordTextBox.Text);
            if (sessionUser == null)
            {
                MessageBox.Show("Email/parola invalida sau cont inactiv.", "Login esuat");
                return;
            }

            ApplicationSession.SignIn(sessionUser);
            ThemeManager.SetTheme(
                string.Equals(sessionUser.ThemePreference, "light", StringComparison.OrdinalIgnoreCase)
                    ? AppTheme.Light
                    : AppTheme.Dark);
            Hide();
            using (var dashboard = new DashboardForm(_services))
            {
                dashboard.ShowDialog();
            }

            ApplicationSession.SignOut();
            _passwordTextBox.Text = string.Empty;
            Show();
        }

        private void OnRegisterClick(object sender, EventArgs e)
        {
            using (var registerForm = new RegisterForm(_services))
            {
                Hide();
                registerForm.ShowDialog(this);
                Show();
            }
        }
    }
}
