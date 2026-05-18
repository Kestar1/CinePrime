using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace CinePrime.UI
{
    public enum AppTheme
    {
        Dark,
        Light
    }

    public static class ThemeManager
    {
        public static event Action ThemeChanged;

        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

        public static readonly Font TitleFont = new Font("Segoe UI Variable Display", 30, FontStyle.Bold);
        public static readonly Font SubtitleFont = new Font("Segoe UI Variable Display", 22, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI Variable Text", 12, FontStyle.Regular);
        public static readonly Font ButtonFont = new Font("Segoe UI Variable Text", 11, FontStyle.Bold);
        public static readonly Font CaptionFont = new Font("Segoe UI Variable Text", 10, FontStyle.Regular);
        public static readonly Font BadgeFont = new Font("Segoe UI Variable Text", 9, FontStyle.Bold);
        public static readonly Font MicroFont = new Font("Segoe UI Variable Text", 8.5f, FontStyle.Regular);
        public static readonly Font LargeKpiFont = new Font("Segoe UI Variable Display", 26, FontStyle.Bold);

        public static Color BackgroundColor => GetPalette().Background;
        public static Color SurfaceColor => GetPalette().Surface;
        public static Color CardColor => GetPalette().Card;
        public static Color TextColor => GetPalette().Text;
        public static Color MutedTextColor => GetPalette().Muted;
        public static Color AccentColor => GetPalette().Accent;
        public static Color BorderColor => GetPalette().Border;
        public static Color SuccessColor => GetPalette().Success;
        public static Color WarningColor => GetPalette().Warning;
        public static Color CardHoverColor => GetPalette().CardHover;
        public static Color GlowRedColor => GetPalette().GlowRed;

        public static void SetTheme(AppTheme theme)
        {
            CurrentTheme = theme;
            ThemeChanged?.Invoke();
        }

        public static void Bind(Form form)
        {
            void Handler() => ApplyTheme(form);

            ThemeChanged += Handler;
            form.FormClosed += (_, __) => ThemeChanged -= Handler;
            EnableSmoothRendering(form);
            ApplyTheme(form);
        }

        public static void ApplyTheme(Control root)
        {
            EnableSmoothRendering(root);
            var palette = GetPalette();
            ApplyToControl(root, palette);
        }

        public static void EnableSmoothRendering(Control root)
        {
            SetDoubleBuffered(root);
            foreach (Control child in root.Controls)
            {
                EnableSmoothRendering(child);
            }
        }

        private static (Color Background, Color Surface, Color Card, Color Text, Color Muted, Color Accent, Color Border, Color Success, Color Warning, Color CardHover, Color GlowRed) GetPalette()
        {
            if (CurrentTheme == AppTheme.Light)
            {
                return (
                    ColorTranslator.FromHtml("#F3F5FA"),
                    ColorTranslator.FromHtml("#FFFFFF"),
                    ColorTranslator.FromHtml("#FFFFFF"),
                    ColorTranslator.FromHtml("#111827"),
                    ColorTranslator.FromHtml("#64748B"),
                    ColorTranslator.FromHtml("#D71920"),
                    ColorTranslator.FromHtml("#DDE3EE"),
                    ColorTranslator.FromHtml("#059669"),
                    ColorTranslator.FromHtml("#D97706"),
                    ColorTranslator.FromHtml("#EEF2FF"),
                    ColorTranslator.FromHtml("#E50914"));
            }

            return (
                ColorTranslator.FromHtml("#090A0F"),
                ColorTranslator.FromHtml("#10121B"),
                ColorTranslator.FromHtml("#10121B"),
                ColorTranslator.FromHtml("#F8FAFC"),
                ColorTranslator.FromHtml("#A0A0A0"),
                ColorTranslator.FromHtml("#E50914"),
                ColorTranslator.FromHtml("#252A3A"),
                ColorTranslator.FromHtml("#10B981"),
                ColorTranslator.FromHtml("#F59E0B"),
                ColorTranslator.FromHtml("#161B2E"),
                ColorTranslator.FromHtml("#FF1F2D"));
        }

        private static void ApplyToControl(Control control, (Color Background, Color Surface, Color Card, Color Text, Color Muted, Color Accent, Color Border, Color Success, Color Warning, Color CardHover, Color GlowRed) p)
        {
            if (control is Form)
            {
                control.BackColor = p.Background;
                control.ForeColor = p.Text;
                control.Font = BodyFont;
            }
            else if (control is ToggleSwitch toggle)
            {
                toggle.BackColor = p.Background;
                toggle.ForeColor = p.Text;
                toggle.Invalidate();
            }
            else if (control is PremiumButton premiumButton)
            {
                premiumButton.BackColor = Color.Transparent;
                premiumButton.ForeColor = premiumButton.Variant == PremiumButtonVariant.Secondary && CurrentTheme == AppTheme.Light
                    ? p.Text
                    : Color.White;
                premiumButton.Font = ButtonFont;
                premiumButton.Invalidate();
            }
            else if (control is CinematicBackgroundPanel cinematicBackground)
            {
                cinematicBackground.BackColor = Color.Transparent;
                cinematicBackground.ForeColor = p.Text;
                cinematicBackground.Invalidate();
            }
            else if (control is PremiumScrollPanel scrollPanel)
            {
                scrollPanel.BackColor = Color.Transparent;
                scrollPanel.ForeColor = p.Text;
                scrollPanel.UpdateLayout();
            }
            else if (control is GlassPanel glassPanel)
            {
                glassPanel.BackColor = Color.Transparent;
                glassPanel.ForeColor = p.Text;
                glassPanel.Invalidate();
            }
            else if ((control.Tag?.ToString() ?? string.Empty) == "card")
            {
                control.BackColor = p.Card;
                control.ForeColor = p.Text;
                RoundControl(control, control is SoftPanel ? ((SoftPanel)control).Radius : 18);
            }
            else if ((control.Tag?.ToString() ?? string.Empty) == "transparent")
            {
                control.BackColor = Color.Transparent;
                control.ForeColor = p.Text;
            }
            else if ((control.Tag?.ToString() ?? string.Empty).StartsWith("nav", StringComparison.OrdinalIgnoreCase))
            {
                control.Font = CaptionFont;
            }
            else if (control is Button button)
            {
                var tag = control.Tag?.ToString() ?? string.Empty;
                button.BackColor = tag == "secondary" ? p.Surface : p.Accent;
                button.ForeColor = Color.White;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = tag == "secondary" ? 1 : 0;
                button.FlatAppearance.BorderColor = p.Border;
                button.Font = ButtonFont;
                button.Height = Math.Max(button.Height, 42);
                RoundControl(button, 14);
            }
            else if (control is TextBox || control is ComboBox)
            {
                control.BackColor = p.Surface;
                control.ForeColor = p.Text;
                control.Font = BodyFont;
                control.Margin = new Padding(4, 6, 4, 6);

                if (control is TextBox textBox)
                {
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.Multiline = false;
                    textBox.Height = Math.Max(textBox.Height, 44);
                }

                if (control is ComboBox combo)
                {
                    combo.FlatStyle = FlatStyle.Flat;
                }

                // TextBox/ComboBox use their native inner padding. Region clipping cuts placeholder text.
            }
            else if (control is DataGridView grid)
            {
                grid.BackgroundColor = p.Card;
                grid.ForeColor = p.Text;
                grid.GridColor = p.Border;
                grid.EnableHeadersVisualStyles = false;
                grid.ColumnHeadersDefaultCellStyle.BackColor = p.Surface;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = p.Text;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font(CaptionFont.FontFamily, 10, FontStyle.Bold);
                grid.DefaultCellStyle.BackColor = p.Card;
                grid.DefaultCellStyle.ForeColor = p.Text;
                grid.DefaultCellStyle.SelectionBackColor = CurrentTheme == AppTheme.Light
                    ? ColorTranslator.FromHtml("#FDE8EA")
                    : ColorTranslator.FromHtml("#3B080D");
                grid.DefaultCellStyle.SelectionForeColor = Color.White;
                if (CurrentTheme == AppTheme.Light)
                {
                    grid.DefaultCellStyle.SelectionForeColor = p.Text;
                }
                grid.AlternatingRowsDefaultCellStyle.BackColor = CurrentTheme == AppTheme.Light
                    ? ColorTranslator.FromHtml("#F8FAFC")
                    : ColorTranslator.FromHtml("#131722");
                grid.BorderStyle = BorderStyle.None;
                grid.RowTemplate.Height = 42;
                grid.RowTemplate.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
                grid.ColumnHeadersHeight = 44;
                grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                grid.DefaultCellStyle.Font = CaptionFont;
                grid.Cursor = Cursors.Hand;
                grid.CellMouseEnter -= GridCellMouseEnter;
                grid.CellMouseEnter += GridCellMouseEnter;
                grid.CellMouseLeave -= GridCellMouseLeave;
                grid.CellMouseLeave += GridCellMouseLeave;
            }
            else if (control is Label label)
            {
                var labelTag = control.Tag?.ToString() ?? string.Empty;
                label.ForeColor = labelTag == "accent-text"
                    ? p.Accent
                    : labelTag == "muted-label"
                        ? p.Muted
                        : p.Text;
                label.BackColor = Color.Transparent;
                // If the form didn't explicitly set a font size, keep labels consistent with captions.
                if (label.Font == null || label.Font.Size <= 13)
                {
                    label.Font = CaptionFont;
                }
            }
            else
            {
                control.BackColor = p.Background;
                control.ForeColor = p.Text;
            }

            foreach (Control child in control.Controls)
            {
                ApplyToControl(child, p);
            }
        }

        private static void RoundControl(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            using (var path = CreateRoundPath(new Rectangle(0, 0, control.Width, control.Height), radius))
            {
                control.Region = new Region(path);
            }

            control.Resize -= OnRoundedControlResize;
            control.Resize += OnRoundedControlResize;
        }

        private static void OnRoundedControlResize(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                RoundControl(control, control is Button ? 14 : 18);
            }
        }

        private static GraphicsPath CreateRoundPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void SetDoubleBuffered(Control control)
        {
            if (control is TextBox || control is ComboBox || control is DateTimePicker || control is NumericUpDown)
            {
                return;
            }

            var property = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            property?.SetValue(control, true, null);
        }

        private static void GridCellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView grid || e.RowIndex < 0)
            {
                return;
            }

            grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = CurrentTheme == AppTheme.Dark
                ? ColorTranslator.FromHtml("#1A1F30")
                : ColorTranslator.FromHtml("#F0F4FF");
        }

        private static void GridCellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView grid || e.RowIndex < 0)
            {
                return;
            }

            grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
                ? GetPalette().Card
                : CurrentTheme == AppTheme.Dark
                    ? ColorTranslator.FromHtml("#131722")
                    : ColorTranslator.FromHtml("#F8FAFC");
        }
    }
}
