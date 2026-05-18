using System;
using System.Drawing;
using System.Windows.Forms;
using CinePrime.BLL.Models;

namespace CinePrime.UI.Forms
{
    public class SettingsForm : Form
    {
        private readonly ServiceRegistry _services;
        private Label _currentThemeLabel;

        public SettingsForm(ServiceRegistry services)
        {
            _services = services;
            Text = "Settings (Admin)";
            Width = 980;
            Height = 640;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(24);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Tag = "transparent"
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildTopCards(), 0, 1);
            root.Controls.Add(BuildSettingsCards(), 0, 2);

            Controls.Add(root);
            ThemeManager.Bind(this);
        }

        private Control BuildHeader()
        {
            var wrap = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Tag = "transparent"
            };
            wrap.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            wrap.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            wrap.Controls.Add(new Label
            {
                Text = "Theme, validare si comportament",
                Dock = DockStyle.Fill,
                Font = ThemeManager.SubtitleFont,
                TextAlign = ContentAlignment.BottomLeft
            }, 0, 0);

            _currentThemeLabel = new Label
            {
                Text = $"Tema curenta: {ThemeManager.CurrentTheme}",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                Tag = "muted-label",
                TextAlign = ContentAlignment.TopLeft
            };
            wrap.Controls.Add(_currentThemeLabel, 0, 1);
            return wrap;
        }

        private Control BuildTopCards()
        {
            var cards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3,
                Tag = "transparent"
            };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            cards.Controls.Add(new KpiCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 0),
                Title = "Tema activa",
                Value = ThemeManager.CurrentTheme.ToString(),
                Subtitle = "Dark / Light se aplica live",
                Trend = string.Empty
            }, 0, 0);

            cards.Controls.Add(new KpiCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 6, 0),
                Title = "Validare date",
                Value = "Activa",
                Subtitle = "Verificare la autentificare, CRUD si rezervari",
                Trend = string.Empty
            }, 1, 0);

            cards.Controls.Add(new KpiCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(12, 0, 0, 0),
                Title = "Exceptii",
                Value = "Protejate",
                Subtitle = "Feedback premium la erori si actiuni",
                Trend = string.Empty
            }, 2, 0);

            return cards;
        }

        private Control BuildSettingsCards()
        {
            var cards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Tag = "transparent",
                Padding = new Padding(0, 12, 0, 0)
            };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

            cards.Controls.Add(BuildThemeCard(), 0, 0);
            cards.Controls.Add(BuildQualityCard(), 1, 0);
            return cards;
        }

        private Control BuildThemeCard()
        {
            var card = new SoftPanel
            {
                Dock = DockStyle.Fill,
                Radius = 22,
                Padding = new Padding(24),
                Margin = new Padding(0, 0, 12, 0),
                ShowGradient = true,
                HoverAccent = false
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
                Tag = "transparent"
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(new Label
            {
                Text = "Dark / Light mode",
                Dock = DockStyle.Fill,
                Font = ThemeManager.ButtonFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            layout.Controls.Add(new Label
            {
                Text = "Tema se aplica instant pe toate ecranele si se salveaza pentru utilizatorul curent.",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                Tag = "muted-label",
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            var toggleWrap = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Tag = "transparent"
            };
            toggleWrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            toggleWrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            toggleWrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            toggleWrap.Controls.Add(new Label
            {
                Text = "Dark mode",
                Dock = DockStyle.Fill,
                Font = ThemeManager.ButtonFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var themeToggle = new ToggleSwitch
            {
                Checked = ThemeManager.CurrentTheme == AppTheme.Dark,
                Margin = new Padding(0, 6, 0, 6),
                Anchor = AnchorStyles.Left
            };
            themeToggle.CheckedChanged += (_, __) =>
            {
                try
                {
                    ThemeManager.SetTheme(themeToggle.Checked ? AppTheme.Dark : AppTheme.Light);
                    SaveThemePreference(themeToggle.Checked ? "dark" : "light");
                    _currentThemeLabel.Text = $"Tema curenta: {ThemeManager.CurrentTheme}";
                    ToastNotification.Show(this, $"Tema a fost schimbata pe {ThemeManager.CurrentTheme}.", ToastType.Success);
                }
                catch (Exception ex)
                {
                    UiFeedback.ShowException(this, ex, "Tema nu a putut fi actualizata.");
                }
            };
            toggleWrap.Controls.Add(themeToggle, 1, 0);
            toggleWrap.Controls.Add(new Label
            {
                Text = "Schimba vizualul live, fara restart.",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                Tag = "muted-label",
                TextAlign = ContentAlignment.MiddleLeft
            }, 2, 0);
            layout.Controls.Add(toggleWrap, 0, 2);

            layout.Controls.Add(new Label
            {
                Text = "Recomandare: dark pentru dashboard si analytics, light pentru lucru indelungat in CRUD.",
                Dock = DockStyle.Fill,
                Font = ThemeManager.BodyFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 3);

            card.Controls.Add(layout);
            return card;
        }

        private Control BuildQualityCard()
        {
            var card = new SoftPanel
            {
                Dock = DockStyle.Fill,
                Radius = 22,
                Padding = new Padding(24),
                Margin = new Padding(12, 0, 0, 0),
                ShowGradient = true,
                HoverAccent = false
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 6,
                ColumnCount = 1,
                Tag = "transparent"
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(new Label
            {
                Text = "Calitate functionala",
                Dock = DockStyle.Fill,
                Font = ThemeManager.ButtonFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            layout.Controls.Add(new Label { Text = "Cinema: CinePrime", Dock = DockStyle.Fill, Font = ThemeManager.BodyFont }, 0, 1);
            layout.Controls.Add(new Label { Text = "Contact: contact@cineprime.md", Dock = DockStyle.Fill, Font = ThemeManager.BodyFont }, 0, 2);
            layout.Controls.Add(new Label { Text = "Security: parolele sunt salvate ca SHA-256 hash.", Dock = DockStyle.Fill, Font = ThemeManager.BodyFont }, 0, 3);
            layout.Controls.Add(new Label { Text = "Exceptii: tratate global si local cu feedback premium.", Dock = DockStyle.Fill, Font = ThemeManager.BodyFont }, 0, 4);
            layout.Controls.Add(new Label
            {
                Text = "Aplicatia valideaza datele de autentificare, rezervare, quick sale si editare CRUD inainte de salvare.",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                Tag = "muted-label",
                TextAlign = ContentAlignment.TopLeft
            }, 0, 5);

            card.Controls.Add(layout);
            return card;
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
