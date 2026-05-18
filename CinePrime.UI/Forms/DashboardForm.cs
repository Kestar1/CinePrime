using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CinePrime.BLL.Models;

namespace CinePrime.UI.Forms
{
    public class DashboardForm : Form
    {
        private readonly ServiceRegistry _services;
        private readonly Dictionary<string, Button> _navButtons = new Dictionary<string, Button>();
        private PremiumScrollPanel _contentHost;
        private Label _pageTitleLabel;
        private Label _headerInfoLabel;
        private Timer _clockTimer;
        private Timer _refreshTimer;
        private string _activePage = "Dashboard";

        public DashboardForm(ServiceRegistry services)
        {
            _services = services;
            Text = "CinePrime - Dashboard";
            Width = 1280;
            Height = 820;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1180, 760);
            Padding = new Padding(0);
            Opacity = 1.0;

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            shell.Controls.Add(CreateSidebar(), 0, 0);
            shell.Controls.Add(CreateMainArea(), 1, 0);
            Controls.Add(shell);

            ThemeManager.Bind(this);
            if (ApplicationSession.IsAdmin)
            {
                ShowDashboard();
            }
            else
            {
                ShowOperatorHome();
            }
            StartClock();
            StartAutoRefresh();
        }

        private Control CreateSidebar()
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BackColor = ColorTranslator.FromHtml("#07080D")
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 12,
                Padding = new Padding(0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            for (var i = 1; i <= 9; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            }
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

            var brand = new Label
            {
                Text = "CinePrime",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(30, 0, 0, 0),
                Font = new Font(ThemeManager.ButtonFont.FontFamily, 15, FontStyle.Bold),
                Tag = "accent-text",
                BackColor = Color.Transparent
            };
            layout.Controls.Add(brand, 0, 0);

            var row = 1;
            AddNav(layout, row++, ApplicationSession.IsAdmin ? "Dashboard" : "Home", ApplicationSession.IsAdmin ? (Action)ShowDashboard : ShowOperatorHome);
            AddNav(layout, row++, "Movies", () => new MoviesForm(_services).ShowDialog(this));
            if (ApplicationSession.IsAdmin)
            {
                AddNav(layout, row++, "Halls", () => new HallsForm(_services).ShowDialog(this));
            }
            AddNav(layout, row++, "Schedule", () => new ScheduleForm(_services).ShowDialog(this));
            AddNav(layout, row++, "Reservations", () => new ReservationsForm(_services).ShowDialog(this));
            AddNav(layout, row++, "Products", () =>
            {
                if (ApplicationSession.IsAdmin)
                {
                    new ProductsForm(_services).ShowDialog(this);
                    return;
                }

                new OperatorProductsForm(_services).ShowDialog(this);
            });
            if (ApplicationSession.IsAdmin)
            {
                AddNav(layout, row++, "Users", () => new UsersForm(_services).ShowDialog(this));
            }
            AddNav(layout, row++, "Reports", ShowReports);
            if (ApplicationSession.IsAdmin)
            {
                AddNav(layout, row++, "Settings", OpenSettings);
            }
            AddNav(layout, 11, "Logout", Close);

            sidebar.Controls.Add(layout);
            return sidebar;
        }

        private void AddNav(TableLayoutPanel layout, int row, string text, Action action)
        {
            var button = new Button
            {
                Tag = "nav",
                Text = "     " + text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.CaptionFont,
                Margin = new Padding(0),
                Padding = new Padding(20, 0, 0, 0),
                Enabled = action != null,
                Cursor = action == null ? Cursors.Default : Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, __) => action?.Invoke();
            button.MouseEnter += (_, __) =>
            {
                if (_activePage != text && button.Enabled)
                {
                    button.BackColor = ColorTranslator.FromHtml("#121520");
                    button.ForeColor = Color.White;
                }
            };
            button.MouseLeave += (_, __) => ApplyNavStyle();

            _navButtons[text] = button;
            layout.Controls.Add(button, 0, row);
        }

        private Control CreateMainArea()
        {
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(26, 14, 26, 24)
            };
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            main.Controls.Add(CreateHeader(), 0, 0);

            _contentHost = new PremiumScrollPanel
            {
                Dock = DockStyle.Fill,
                MinimumContentHeight = 910
            };
            main.Controls.Add(_contentHost, 0, 1);
            return main;
        }

        private Control CreateHeader()
        {
            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Tag = "transparent" };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));

            _pageTitleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.SubtitleFont.FontFamily, 17, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            header.Controls.Add(_pageTitleLabel, 0, 0);
            var rightHeader = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, Tag = "transparent" };
            rightHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            rightHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            rightHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
            rightHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            rightHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            rightHeader.Controls.Add(new Label
            {
                Text = "CinePrime",
                Tag = "accent-text",
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.TitleFont.FontFamily, 15, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            }, 1, 0);
            var avatar = new UserAvatarBadge
            {
                Dock = DockStyle.Fill,
                Initial = ApplicationSession.CurrentUser?.FullName ?? "A",
                Margin = new Padding(0)
            };
            rightHeader.Controls.Add(avatar, 2, 0);
            var userLabel = new Label
            {
                Text = ApplicationSession.CurrentUser?.FullName ?? string.Empty,
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };
            rightHeader.Controls.Add(userLabel, 3, 0);
            _headerInfoLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };
            UpdateHeaderClock();
            rightHeader.Controls.Add(_headerInfoLabel, 4, 0);
            header.Controls.Add(rightHeader, 1, 0);
            return header;
        }

        private void ShowOperatorHome()
        {
            _activePage = ApplicationSession.IsAdmin ? "Dashboard" : "Home";
            _pageTitleLabel.Text = ApplicationSession.IsAdmin ? "Dashboard" : "Home - Operator";
            SetContent(CreateOperatorHomeContent());
            ApplyNavStyle();
        }

        private Control CreateOperatorHomeContent()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(0)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var kpis = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(0, 18, 0, 14) };
            for (var i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            kpis.Controls.Add(CreateKpiCard("Vanzari bilete azi", GetTicketSalesToday().ToString("0.00") + " MDL", "Rezervari confirmate", "+8%"), 0, 0);
            kpis.Controls.Add(CreateKpiCard("Produse vandute azi", GetProductSalesToday().ToString("0.00") + " MDL", "Snack & Drink", "+3%"), 1, 0);
            kpis.Controls.Add(CreateKpiCard("Ocupare saptamanala", GetWeeklyOccupancy().ToString("0") + "%", "Locuri rezervate", "+5%"), 2, 0);
            kpis.Controls.Add(CreateKpiCard("Top film", GetTopMovieTitle(), "Cele mai multe vizualizari"), 3, 0);
            root.Controls.Add(kpis, 0, 0);

            var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 10, 0, 12) };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var actions = new SoftPanel { Dock = DockStyle.Fill, Padding = new Padding(24), Margin = new Padding(0, 0, 12, 0) };
            var actionsLayout = new TableLayoutPanel { Tag = "transparent", Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1 };
            actionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            actionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            actionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            actionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            actionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            actionsLayout.Controls.Add(MakeLabel("Quick Actions", ThemeManager.SubtitleFont), 0, 0);
            var resBtn = new PremiumButton { Text = "Rezervare noua cu locuri", Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
            resBtn.Click += (_, __) => new SeatReservationForm(_services).ShowDialog(this);
            actionsLayout.Controls.Add(resBtn, 0, 1);

            var saleBtn = new PremiumButton { Text = "Vanzare rapida produse", Variant = PremiumButtonVariant.Secondary, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
            saleBtn.Click += (_, __) => new QuickSaleForm(_services).ShowDialog(this);
            actionsLayout.Controls.Add(saleBtn, 0, 2);

            var viewResBtn = new PremiumButton { Text = "Vizualizare rezervari", Variant = PremiumButtonVariant.Secondary, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
            viewResBtn.Click += (_, __) => new ReservationsForm(_services).ShowDialog(this);
            actionsLayout.Controls.Add(viewResBtn, 0, 3);

            actions.Controls.Add(actionsLayout);
            main.Controls.Add(actions, 0, 0);

            var info = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Tag = "transparent" };
            info.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            info.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            info.Controls.Add(CreateNowPlayingCard(), 0, 0);
            info.Controls.Add(CreateSystemStatusCard(), 0, 1);
            main.Controls.Add(info, 1, 0);

            root.Controls.Add(main, 0, 1);
            return root;
        }

        private void ShowDashboard()
        {
            _activePage = "Dashboard";
            _pageTitleLabel.Text = "Dashboard";
            SetContent(CreateDashboardContent());
            ApplyNavStyle();
        }

        private void ShowReports()
        {
            _activePage = "Reports";
            _pageTitleLabel.Text = "Reports";
            SetContent(CreateReportsContent());
            ApplyNavStyle();
        }

        private void OpenSettings()
        {
            using (var form = new SettingsForm(_services))
            {
                form.ShowDialog(this);
            }

            ThemeManager.ApplyTheme(this);
            ApplyNavStyle();
            UpdateHeaderClock();
        }

        private void SetContent(Control content)
        {
            _contentHost.SuspendLayout();
            _contentHost.Content.Controls.Clear();
            _contentHost.Content.Controls.Add(content);
            _contentHost.UpdateLayout();
            _contentHost.ScrollToTop();
            _contentHost.ResumeLayout();
            ThemeManager.ApplyTheme(_contentHost);
        }

        private void ApplyNavStyle()
        {
            foreach (var pair in _navButtons)
            {
                var active = pair.Key == _activePage;
                pair.Value.Tag = active ? "nav-active" : "nav";
                pair.Value.BackColor = active ? ColorTranslator.FromHtml("#4A070D") : ColorTranslator.FromHtml("#07080D");
                pair.Value.ForeColor = active ? Color.White : ThemeManager.MutedTextColor;
            }
        }

        private Control CreateDashboardContent()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(0)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 250));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var kpis = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(0, 18, 0, 14) };
            for (var i = 0; i < 4; i++)
            {
                kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }
            kpis.Controls.Add(CreateKpiCard("Vanzari bilete azi", GetTicketSalesToday().ToString("0.00") + " MDL", "Rezervari confirmate", "+8%"), 0, 0);
            kpis.Controls.Add(CreateKpiCard("Popcorn azi", GetPopcornQuantityToday().ToString(), "Produse vandute", "+3%"), 1, 0);
            kpis.Controls.Add(CreateKpiCard("Ocupare saptamanala", GetWeeklyOccupancy().ToString("0") + "%", "Locuri rezervate", "+5%"), 2, 0);
            kpis.Controls.Add(CreateKpiCard("Top film", GetTopMovieTitle(), "Rating maxim"), 3, 0);
            root.Controls.Add(kpis, 0, 0);

            root.Controls.Add(CreateInfoGrid(), 0, 1);

            var charts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 10, 0, 0) };
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            charts.Controls.Add(new DashboardChartPanel
            {
                Dock = DockStyle.Fill,
                ChartTitle = "Vanzari pe categorii",
                Values = new[] { GetTicketSalesToday(), GetProductSalesToday(), GetTotalSalesToday() },
                Labels = new[] { "Bilete", "Snack", "Total" },
                ValueSuffix = " MDL",
                Margin = new Padding(0, 0, 10, 0)
            }, 0, 0);
            charts.Controls.Add(new DashboardChartPanel
            {
                Dock = DockStyle.Fill,
                ChartTitle = "Ocupare estimata",
                Values = GetOccupancyByDay(),
                Labels = new[] { "L", "M", "M", "J", "V", "S", "D" },
                DrawLine = true,
                ValueSuffix = "%",
                Margin = new Padding(10, 0, 0, 0)
            }, 1, 0);
            root.Controls.Add(charts, 0, 2);
            return root;
        }

        private Control CreateKpiCard(string title, string value, string subtitle, string trend = "")
        {
            return new KpiCard
            {
                Title = title,
                Value = value,
                Subtitle = subtitle,
                Trend = trend,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 14, 0)
            };
        }

        private Control CreateInfoGrid()
        {
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 10, 0, 12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
            grid.Controls.Add(CreateNowPlayingCard(), 0, 0);
            grid.Controls.Add(CreateQuickActionsCard(), 1, 0);
            grid.Controls.Add(CreateSystemStatusCard(), 2, 0);
            return grid;
        }

        private Control CreateNowPlayingCard()
        {
            var movie = _services.DbContext.Movies
                .Where(m => string.Equals(m.Status, "active", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(m => m.Rating)
                .FirstOrDefault();
            var title = movie?.Title ?? "N/A";
            var details = movie == null ? "Nu exista filme active" : $"{movie.Genre} | {movie.DurationMinutes} min | rating {movie.Rating:0.0}";
            return CreateWideInfoCard("Now Playing", title, details);
        }

        private Control CreateQuickActionsCard()
        {
            var card = CreateDashboardCard(new Padding(0, 0, 18, 0), new Padding(24, 20, 24, 20));
            var layout = new TableLayoutPanel { Tag = "transparent", Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(MakeLabel("Quick Actions", ThemeManager.CaptionFont), 0, 0);

            var reservation = new PremiumButton { Text = "Rezervare cu locuri", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 8) };
            reservation.Click += (_, __) => new SeatReservationForm(_services).ShowDialog(this);
            layout.Controls.Add(reservation, 0, 1);

            var sale = new PremiumButton { Text = "Quick Sale", Variant = PremiumButtonVariant.Secondary, Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 8) };
            sale.Click += (_, __) => new QuickSaleForm(_services).ShowDialog(this);
            layout.Controls.Add(sale, 0, 2);
            card.Controls.Add(layout);
            return card;
        }

        private Control CreateSystemStatusCard()
        {
            var activeHalls = _services.DbContext.Halls.Count(h => string.Equals(h.Status, "active", StringComparison.OrdinalIgnoreCase));
            var activeSchedules = _services.DbContext.Schedules.Count(s => s.StartTime.Date == DateTime.Today && s.Status != "cancelled");
            var lowStock = _services.DbContext.Products.Count(p => p.StockQuantity <= p.MinStockAlert);
            var card = CreateDashboardCard(new Padding(0, 0, 18, 0), new Padding(28, 20, 28, 20));
            var layout = new TableLayoutPanel { Tag = "transparent", Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.4f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.Controls.Add(MakeLabel("System Status", ThemeManager.CaptionFont), 0, 0);
            layout.Controls.Add(MakeLabel($"{activeHalls} sali active", new Font(ThemeManager.SubtitleFont.FontFamily, 20, FontStyle.Bold)), 0, 1);
            layout.Controls.Add(MakeLabel($"{activeSchedules} programari azi", new Font(ThemeManager.SubtitleFont.FontFamily, 20, FontStyle.Bold)), 0, 2);
            layout.Controls.Add(MakeLabel($"{lowStock} produse cu stoc minim", new Font(ThemeManager.SubtitleFont.FontFamily, 20, FontStyle.Bold)), 0, 3);
            layout.Controls.Add(MakeLabel("Date actualizate automat", ThemeManager.CaptionFont), 0, 4);
            card.Controls.Add(layout);
            return card;
        }

        private Control CreateWideInfoCard(string title, string value, string subtitle)
        {
            var card = CreateDashboardCard(new Padding(0, 0, 18, 0), new Padding(28, 20, 28, 20));
            var layout = new TableLayoutPanel { Tag = "transparent", Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.Controls.Add(MakeLabel(title, ThemeManager.CaptionFont), 0, 0);
            layout.Controls.Add(MakeLabel(value, new Font(ThemeManager.SubtitleFont.FontFamily, 24, FontStyle.Bold)), 0, 1);
            layout.Controls.Add(MakeLabel(subtitle, ThemeManager.CaptionFont), 0, 2);
            card.Controls.Add(layout);
            return card;
        }

        private SoftPanel CreateDashboardCard(Padding margin, Padding padding)
        {
            var card = new SoftPanel
            {
                Dock = DockStyle.Fill,
                Margin = margin,
                Padding = padding,
                ShowGradient = true
            };

            return card;
        }

        private Control CreateReportsContent()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 8) };
            if (ApplicationSession.IsAdmin)
            {
                var exportHtml = new PremiumButton { Text = "Export HTML", Width = 150, Height = 42 };
                exportHtml.Click += (_, __) => ExportReport("html");
                var exportTxt = new PremiumButton { Text = "Export TXT", Variant = PremiumButtonVariant.Secondary, Width = 150, Height = 42, Margin = new Padding(10, 0, 0, 0) };
                exportTxt.Click += (_, __) => ExportReport("txt");
                toolbar.Controls.Add(exportHtml);
                toolbar.Controls.Add(exportTxt);
            }
            else
            {
                toolbar.Controls.Add(new Label
                {
                    Text = "Export disponibil doar pentru administrator",
                    AutoSize = true,
                    Font = ThemeManager.CaptionFont,
                    TextAlign = ContentAlignment.MiddleRight,
                    Margin = new Padding(0, 12, 0, 0)
                });
            }
            root.Controls.Add(toolbar, 0, 0);

            var summary = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 10, 0, 10) };
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            summary.Controls.Add(CreateWideInfoCard("Total Sales", (GetTicketSalesToday() + GetProductSalesToday()).ToString("0.00") + " MDL", "Venituri inregistrate azi"), 0, 0);
            summary.Controls.Add(CreateWideInfoCard("Reservations", _services.DbContext.Reservations.Count.ToString(), "Total rezervari"), 1, 0);
            summary.Controls.Add(CreateWideInfoCard("Products", _services.DbContext.Products.Count.ToString(), "Produse gestionate"), 2, 0);
            root.Controls.Add(summary, 0, 1);

            var charts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            charts.Controls.Add(new DashboardChartPanel
            {
                Dock = DockStyle.Fill,
                ChartTitle = "Revenue Breakdown",
                Values = new[] { GetTicketSalesToday(), GetProductSalesToday(), GetTotalSalesToday() },
                Labels = new[] { "Tickets", "Products", "Total" },
                ValueSuffix = " MDL",
                Margin = new Padding(0, 0, 10, 0)
            }, 0, 0);
            charts.Controls.Add(new DashboardChartPanel
            {
                Dock = DockStyle.Fill,
                ChartTitle = "Weekly Occupancy",
                Values = GetOccupancyByDay(),
                Labels = new[] { "L", "M", "M", "J", "V", "S", "D" },
                DrawLine = true,
                ValueSuffix = "%",
                Margin = new Padding(10, 0, 0, 0)
            }, 1, 0);
            root.Controls.Add(charts, 0, 2);
            return root;
        }

        private static Label MakeLabel(string text, Font font)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = font,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };
        }

        private void StartClock()
        {
            _clockTimer = new Timer { Interval = 1000 };
            _clockTimer.Tick += (_, __) => UpdateHeaderClock();
            _clockTimer.Start();
            FormClosed += (_, __) =>
            {
                _clockTimer.Stop();
                _clockTimer.Dispose();
            };
        }

        private void StartAutoRefresh()
        {
            _refreshTimer = new Timer { Interval = 30000 };
            _refreshTimer.Tick += (_, __) =>
            {
                if (_activePage == "Dashboard" || _activePage == "Home")
                {
                    if (ApplicationSession.IsAdmin)
                        SetContent(CreateDashboardContent());
                    else
                        SetContent(CreateOperatorHomeContent());
                }
            };
            _refreshTimer.Start();
            FormClosed += (_, __) =>
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            };
        }

        private void ExportReport(string format)
        {
            using (var dialog = new SaveFileDialog
            {
                Filter = format == "html" ? "HTML report (*.html)|*.html" : "Text report (*.txt)|*.txt",
                FileName = $"cineprime_report_{DateTime.Now:yyyyMMdd_HHmm}.{format}"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var content = format == "html" ? BuildHtmlReport() : BuildTextReport();
                File.WriteAllText(dialog.FileName, content, Encoding.UTF8);
                MessageBox.Show("Raport exportat cu succes.", "Succes");
            }
        }

        private string BuildTextReport()
        {
            var activeReservations = _services.DbContext.Reservations.Count(r => r.Status != "cancelled");
            return
                "CinePrime - Raport zilnic" + Environment.NewLine +
                $"Data: {DateTime.Now:dd.MM.yyyy HH:mm}" + Environment.NewLine +
                $"Vanzari bilete: {GetTicketSalesToday():0.00} MDL" + Environment.NewLine +
                $"Vanzari produse: {GetProductSalesToday():0.00} MDL" + Environment.NewLine +
                $"Total: {GetTotalSalesToday():0.00} MDL" + Environment.NewLine +
                $"Rezervari active: {activeReservations}" + Environment.NewLine +
                $"Top film: {GetTopMovieTitle()}" + Environment.NewLine +
                $"Ocupare saptamanala: {GetWeeklyOccupancy():0.##}%";
        }

        private string BuildHtmlReport()
        {
            var activeReservations = _services.DbContext.Reservations.Count(r => r.Status != "cancelled");
            return $@"<!doctype html>
<html>
<head><meta charset=""utf-8""><title>CinePrime Report</title>
<style>body{{font-family:Segoe UI,Arial;background:#0b0b10;color:#fff;padding:32px}}.card{{background:#181b27;border-radius:14px;padding:18px;margin:12px 0}}h1{{color:#e50914}}</style></head>
<body>
<h1>CinePrime - Raport zilnic</h1>
<div class=""card"">Data: {DateTime.Now:dd.MM.yyyy HH:mm}</div>
<div class=""card"">Vanzari bilete: {GetTicketSalesToday():0.00} MDL</div>
<div class=""card"">Vanzari produse: {GetProductSalesToday():0.00} MDL</div>
<div class=""card"">Total: {GetTotalSalesToday():0.00} MDL</div>
<div class=""card"">Rezervari active: {activeReservations}</div>
<div class=""card"">Top film: {GetTopMovieTitle()}</div>
<div class=""card"">Ocupare saptamanala: {GetWeeklyOccupancy():0.##}%</div>
</body></html>";
        }

        private void UpdateHeaderClock()
        {
            if (_headerInfoLabel == null)
            {
                return;
            }

            _headerInfoLabel.Text = $"{DateTime.Now:dddd, MMMM d, yyyy HH:mm:ss}";
        }


        private decimal GetTicketSalesToday()
        {
            return _services.DbContext.Sales
                .Where(s => s.CreatedAt.Date == DateTime.Today && (s.SaleType == "tickets" || s.SaleType == "mixed"))
                .Sum(s => s.TotalAmount);
        }

        private int GetPopcornQuantityToday()
        {
            var popcornIds = _services.DbContext.Products
                .Where(p => p.Name.IndexOf("popcorn", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(p => p.Id)
                .ToList();
            var todaySaleIds = _services.DbContext.Sales
                .Where(s => s.CreatedAt.Date == DateTime.Today)
                .Select(s => s.Id)
                .ToList();

            return _services.DbContext.SaleItems
                .Where(i => i.ProductId.HasValue && popcornIds.Contains(i.ProductId.Value) && todaySaleIds.Contains(i.SaleId))
                .Sum(i => i.Quantity);
        }

        private decimal GetWeeklyOccupancy()
        {
            var weekStart = DateTime.Today.AddDays(-6);
            var scheduleIds = _services.DbContext.Schedules
                .Where(s => s.StartTime.Date >= weekStart && s.StartTime.Date <= DateTime.Today.AddDays(7))
                .Select(s => s.Id)
                .ToList();
            var reserved = _services.DbContext.ReservationSeats
                .Where(seat => _services.DbContext.Reservations.Any(r => r.Id == seat.ReservationId && scheduleIds.Contains(r.ScheduleId) && r.Status != "cancelled"))
                .Count();
            var capacity = _services.DbContext.Schedules
                .Where(s => scheduleIds.Contains(s.Id))
                .Sum(s => _services.DbContext.Halls.FirstOrDefault(h => h.Id == s.HallId)?.Capacity ?? 0);

            return capacity == 0 ? 0 : reserved * 100m / capacity;
        }

        private string GetTopMovieTitle()
        {
            return _services.DbContext.Movies
                .OrderByDescending(m => m.Rating)
                .ThenByDescending(m => m.TotalReviews)
                .FirstOrDefault()?.Title ?? "N/A";
        }

        private decimal GetProductSalesToday()
        {
            return _services.DbContext.Sales
                .Where(s => s.CreatedAt.Date == DateTime.Today && (s.SaleType == "products" || s.SaleType == "mixed"))
                .Sum(s => s.TotalAmount);
        }

        private decimal GetTotalSalesToday()
        {
            return GetTicketSalesToday() + GetProductSalesToday();
        }

        private decimal[] GetOccupancyByDay()
        {
            var startOfWeek = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek + 6) % 7);
            var values = new decimal[7];
            for (var day = 0; day < 7; day++)
            {
                var date = startOfWeek.AddDays(day);
                var schedules = _services.DbContext.Schedules.Where(s => s.StartTime.Date == date.Date).ToList();
                var scheduleIds = schedules.Select(s => s.Id).ToList();
                var reserved = _services.DbContext.ReservationSeats
                    .Where(seat => _services.DbContext.Reservations.Any(r =>
                        r.Id == seat.ReservationId &&
                        scheduleIds.Contains(r.ScheduleId) &&
                        r.Status != "cancelled"))
                    .Count();
                var capacity = schedules.Sum(s => _services.DbContext.Halls.FirstOrDefault(h => h.Id == s.HallId)?.Capacity ?? 0);
                values[day] = capacity == 0 ? 0 : reserved * 100m / capacity;
            }

            return values;
        }
    }
}
