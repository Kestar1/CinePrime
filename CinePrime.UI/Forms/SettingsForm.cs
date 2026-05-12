using System.Windows.Forms;
using CinePrime.BLL.Models;

namespace CinePrime.UI.Forms
{
    public class SettingsForm : Form
    {
        private readonly ServiceRegistry _services;
        private readonly Label _currentThemeLabel;

        public SettingsForm(ServiceRegistry services)
        {
            _services = services;
            Text = "Settings (Admin)";
            Width = 760;
            Height = 520;
            Padding = new Padding(24);

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 8,
                ColumnCount = 2,
                Padding = new Padding(30),
                Tag = "card"
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var title = new Label
            {
                Text = "Theme Settings",
                Font = ThemeManager.SubtitleFont,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            container.SetColumnSpan(title, 2);
            container.Controls.Add(title, 0, 0);

            _currentThemeLabel = new Label
            {
                Text = $"Tema curenta: {ThemeManager.CurrentTheme}",
                Dock = DockStyle.Fill,
                Font = ThemeManager.BodyFont,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            container.SetColumnSpan(_currentThemeLabel, 2);
            container.Controls.Add(_currentThemeLabel, 0, 1);

            var darkButton = new PremiumButton { Text = "Dark mode", Dock = DockStyle.Fill, Margin = new Padding(0, 6, 8, 6) };
            darkButton.Click += (_, __) =>
            {
                ThemeManager.SetTheme(AppTheme.Dark);
                SaveThemePreference("dark");
                _currentThemeLabel.Text = $"Tema curenta: {ThemeManager.CurrentTheme}";
            };
            container.Controls.Add(darkButton, 0, 2);

            var lightButton = new PremiumButton { Text = "Light mode", Variant = PremiumButtonVariant.Secondary, Dock = DockStyle.Fill, Margin = new Padding(8, 6, 0, 6) };
            lightButton.Click += (_, __) =>
            {
                ThemeManager.SetTheme(AppTheme.Light);
                SaveThemePreference("light");
                _currentThemeLabel.Text = $"Tema curenta: {ThemeManager.CurrentTheme}";
            };
            container.Controls.Add(lightButton, 1, 2);

            var generalTitle = new Label { Text = "General", Font = ThemeManager.SubtitleFont, Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            container.SetColumnSpan(generalTitle, 2);
            container.Controls.Add(generalTitle, 0, 3);
            container.Controls.Add(new Label { Text = "Cinema: CinePrime", Dock = DockStyle.Fill, Font = ThemeManager.BodyFont }, 0, 4);
            container.Controls.Add(new Label { Text = "Contact: contact@cineprime.md", Dock = DockStyle.Fill, Font = ThemeManager.BodyFont }, 1, 4);
            container.Controls.Add(new Label { Text = "Security: parolele sunt salvate ca SHA-256 hash.", Dock = DockStyle.Fill, Font = ThemeManager.BodyFont }, 0, 5);
            container.Controls.Add(new Label { Text = "About: CinePrime Desktop v1.0", Dock = DockStyle.Fill, Font = ThemeManager.BodyFont }, 1, 5);

            Controls.Add(container);
            ThemeManager.Bind(this);
        }

        private void SaveThemePreference(string theme)
        {
            var user = _services.UserRepository.GetById(ApplicationSession.CurrentUser?.Id ?? 0);
            if (user == null)
            {
                return;
            }

            user.ThemePreference = theme;
            _services.UserRepository.Update(user);
            ApplicationSession.CurrentUser.ThemePreference = theme;
        }
    }
}
