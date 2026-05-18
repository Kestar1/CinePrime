using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CinePrime.UI
{
    public enum PremiumButtonVariant
    {
        Primary,
        Secondary,
        Ghost,
        Danger
    }

    public class PremiumButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public PremiumButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            Height = 44;
            Font = ThemeManager.ButtonFont;
            ForeColor = Color.White;
            Padding = new Padding(14, 0, 14, 0);
        }

        public PremiumButtonVariant Variant { get; set; } = PremiumButtonVariant.Primary;

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            using (var path = RoundedPath(new Rectangle(0, 0, Width, Height), 14))
            {
                Region = new Region(path);
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var shift = _pressed ? 1 : 0;

            if (ThemeManager.CurrentTheme == AppTheme.Dark &&
                (Variant == PremiumButtonVariant.Primary || Variant == PremiumButtonVariant.Danger) &&
                !_pressed)
            {
                var shadowRect = new Rectangle(3, 5, Width - 4, Height - 2);
                using (var shadow = new SolidBrush(Color.FromArgb(80, ColorTranslator.FromHtml("#E50914"))))
                using (var shadowPath = RoundedPath(shadowRect, 14))
                {
                    g.FillPath(shadow, shadowPath);
                }
            }

            using (var path = RoundedPath(rect, 14))
            {
                if (Variant == PremiumButtonVariant.Primary || Variant == PremiumButtonVariant.Danger)
                {
                    var start = Variant == PremiumButtonVariant.Danger
                        ? ColorTranslator.FromHtml("#E50914")
                        : ColorTranslator.FromHtml("#FF1F2D");
                    var end = ColorTranslator.FromHtml("#B20710");
                    if (_hover)
                    {
                        start = ControlPaint.Light(start, 0.08f);
                    }

                    using (var brush = new LinearGradientBrush(rect, start, end, LinearGradientMode.Horizontal))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else if (Variant == PremiumButtonVariant.Ghost)
                {
                    using (var brush = new SolidBrush(Color.Transparent))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    var fill = ThemeManager.CurrentTheme == AppTheme.Light
                        ? (_hover ? ColorTranslator.FromHtml("#F4D9DC") : ThemeManager.SurfaceColor)
                        : (_hover ? ThemeManager.BorderColor : ThemeManager.SurfaceColor);
                    using (var brush = new SolidBrush(fill))
                    {
                        g.FillPath(brush, path);
                    }
                }

                var border = _hover ? ThemeManager.AccentColor : ThemeManager.BorderColor;
                using (var pen = new Pen(border, _hover ? 1.5f : 1f))
                {
                    g.DrawPath(pen, path);
                }
            }

            var resolvedForeColor = ForeColor;
            if (Variant == PremiumButtonVariant.Secondary && ThemeManager.CurrentTheme == AppTheme.Light)
            {
                resolvedForeColor = ThemeManager.TextColor;
            }

            var textColor = Enabled ? resolvedForeColor : ThemeManager.MutedTextColor;
            var textRect = new Rectangle(0, shift, Width, Height);
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                textRect,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
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
    }

    public class CinematicBackgroundPanel : Panel
    {
        public Image BackgroundPhoto { get; set; }

        public CinematicBackgroundPanel()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (BackgroundPhoto != null)
            {
                DrawCoverImage(g, BackgroundPhoto, ClientRectangle);
            }
            else
            using (var brush = new LinearGradientBrush(ClientRectangle, ColorTranslator.FromHtml("#C90E17"), ColorTranslator.FromHtml("#430008"), 32f))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            using (var sweep = new LinearGradientBrush(ClientRectangle, Color.FromArgb(BackgroundPhoto == null ? 120 : 62, ColorTranslator.FromHtml("#FF3341")), Color.FromArgb(BackgroundPhoto == null ? 110 : 94, ColorTranslator.FromHtml("#090A0F")), 135f))
            using (var red = new SolidBrush(Color.FromArgb(140, ColorTranslator.FromHtml("#FF2330"))))
            using (var deepRed = new SolidBrush(Color.FromArgb(115, ColorTranslator.FromHtml("#9D0912"))))
            using (var wine = new SolidBrush(Color.FromArgb(128, ColorTranslator.FromHtml("#5A0008"))))
            using (var soft = new SolidBrush(Color.FromArgb(18, Color.White)))
            using (var vignette = new LinearGradientBrush(ClientRectangle, Color.FromArgb(0, Color.Black), Color.FromArgb(120, Color.Black), 90f))
            using (var linePen = new Pen(Color.FromArgb(28, Color.White), 1))
            {
                g.FillRectangle(sweep, ClientRectangle);
                if (BackgroundPhoto == null)
                {
                    g.FillEllipse(red, new Rectangle(-Width / 5, Height / 10, Width / 2, Height / 2));
                    g.FillEllipse(deepRed, new Rectangle(Width - Width / 3, -Height / 6, Width / 2, Height / 2));
                    g.FillEllipse(wine, new Rectangle(Width / 4, Height / 3, Width / 2, Height / 2));
                    g.FillEllipse(soft, new Rectangle(Width / 3, Height - Height / 3, Width / 3, Height / 3));
                    DrawCinemaHallScene(g);
                    DrawSmokeLayer(g, 0.22f, Height * 0.38f, Height * 0.14f, Color.FromArgb(36, Color.White));
                    DrawSmokeLayer(g, 0.58f, Height * 0.56f, Height * 0.18f, Color.FromArgb(28, Color.White));
                    DrawSmokeLayer(g, 0.78f, Height * 0.28f, Height * 0.12f, Color.FromArgb(22, Color.White));
                }
                g.FillRectangle(vignette, ClientRectangle);

                for (var x = -Height; x < Width; x += 90)
                {
                    g.DrawLine(linePen, x, Height, x + Height, 0);
                }

                var rng = new Random(42);
                for (var i = 0; i < 200; i++)
                {
                    var x = rng.Next(0, Math.Max(1, Width));
                    var y = rng.Next(0, Math.Max(1, Height));
                    var alpha = rng.Next(4, 18);
                    using (var grainBrush = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    {
                        g.FillRectangle(grainBrush, x, y, 1, 1);
                    }
                }
            }
        }

        private void DrawCoverImage(Graphics g, Image image, Rectangle bounds)
        {
            var scale = Math.Max(bounds.Width / (float)image.Width, bounds.Height / (float)image.Height);
            var drawWidth = image.Width * scale;
            var drawHeight = image.Height * scale;
            var x = bounds.X + (bounds.Width - drawWidth) / 2f;
            var y = bounds.Y + (bounds.Height - drawHeight) / 2f;
            g.DrawImage(image, x, y, drawWidth, drawHeight);
        }

        private void DrawCinemaHallScene(Graphics g)
        {
            var screenRect = new Rectangle(
                Width / 2 - Width / 7,
                Height / 6,
                Width / 3,
                Height / 4);
            using (var glowBrush = new SolidBrush(Color.FromArgb(70, Color.White)))
            using (var innerBrush = new SolidBrush(Color.FromArgb(245, 255, 248, 240)))
            using (var screenBorder = new Pen(Color.FromArgb(180, ColorTranslator.FromHtml("#FF6A72")), 2f))
            {
                for (var i = 0; i < 6; i++)
                {
                    g.FillRectangle(glowBrush, Rectangle.Inflate(screenRect, i * 10, i * 8));
                }

                g.FillRectangle(innerBrush, screenRect);
                g.DrawRectangle(screenBorder, screenRect);
            }

            var floorPoints = new[]
            {
                new Point(Width / 2 - screenRect.Width / 2, screenRect.Bottom),
                new Point(Width / 2 + screenRect.Width / 2, screenRect.Bottom),
                new Point(Width - Width / 12, Height),
                new Point(Width / 12, Height)
            };
            using (var floorPath = new GraphicsPath())
            using (var floorBrush = new PathGradientBrush(floorPoints))
            {
                floorPath.AddPolygon(floorPoints);
                floorBrush.CenterColor = Color.FromArgb(46, 255, 120, 120);
                floorBrush.SurroundColors = new[]
                {
                    Color.FromArgb(16, Color.Black),
                    Color.FromArgb(16, Color.Black),
                    Color.FromArgb(80, Color.Black),
                    Color.FromArgb(80, Color.Black)
                };
                g.FillPath(floorBrush, floorPath);
            }

            DrawSeatRows(g);
        }

        private void DrawSeatRows(Graphics g)
        {
            var centerX = Width / 2f;
            var startY = Height * 0.58f;
            var rowCount = 6;
            for (var row = 0; row < rowCount; row++)
            {
                var progress = row / (float)(rowCount - 1);
                var seatCount = 8 + row * 2;
                var seatWidth = 14 + row * 2;
                var seatHeight = 18 + row * 2;
                var spacing = seatWidth + 8;
                var rowWidth = seatCount * spacing;
                var startX = centerX - rowWidth / 2f;
                var y = startY + row * (seatHeight + 14);

                for (var seat = 0; seat < seatCount; seat++)
                {
                    var x = startX + seat * spacing;
                    var backRect = new Rectangle((int)x, (int)y, seatWidth, seatHeight);
                    var seatRect = new Rectangle((int)x - 1, (int)y + seatHeight - 4, seatWidth + 2, seatHeight - 2);

                    using (var backBrush = new SolidBrush(Color.FromArgb((int)(130 - progress * 40), ColorTranslator.FromHtml("#2A0E12"))))
                    using (var seatBrush = new SolidBrush(Color.FromArgb((int)(170 - progress * 35), ColorTranslator.FromHtml("#4A171B"))))
                    using (var edgePen = new Pen(Color.FromArgb(80, ColorTranslator.FromHtml("#9E2A31"))))
                    {
                        FillRoundedRectangle(g, backBrush, backRect, 6);
                        FillRoundedRectangle(g, seatBrush, seatRect, 6);
                        DrawRoundedRectangle(g, edgePen, seatRect, 6);
                    }
                }
            }
        }

        private void DrawSmokeLayer(Graphics g, float startXRatio, float centerY, float radiusY, Color tint)
        {
            for (var i = 0; i < 10; i++)
            {
                var width = (int)(Width * (0.22f + i * 0.04f));
                var height = (int)(radiusY * (1.1f + i * 0.12f));
                var x = (int)(Width * startXRatio) - width / 2 + i * 18;
                var y = (int)centerY - height / 2 + (int)(Math.Sin(i * 0.55f) * 18);
                var alpha = Math.Max(8, tint.A - i * 2);
                using (var brush = new SolidBrush(Color.FromArgb(alpha, tint)))
                {
                    g.FillEllipse(brush, new Rectangle(x, y, width, height));
                }
            }
        }

        private void FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = CreateRoundedRectPath(rect, radius))
            {
                g.FillPath(brush, path);
            }
        }

        private void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using (var path = CreateRoundedRectPath(rect, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class UserAvatarBadge : Control
    {
        private string _initial = "A";

        public UserAvatarBadge()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Width = 34;
            Height = 34;
        }

        public string Initial
        {
            get => _initial;
            set
            {
                _initial = string.IsNullOrWhiteSpace(value) ? "A" : value.Substring(0, 1).ToUpperInvariant();
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var size = Math.Max(18, Math.Min(Width, Height) - 6);
            var rect = new Rectangle(
                (Width - size) / 2,
                (Height - size) / 2,
                size,
                size);
            using (var fill = new SolidBrush(ThemeManager.AccentColor))
            using (var border = new Pen(Color.FromArgb(120, ThemeManager.GlowRedColor), 1.2f))
            {
                e.Graphics.FillEllipse(fill, rect);
                e.Graphics.DrawEllipse(border, rect);
            }

            TextRenderer.DrawText(
                e.Graphics,
                _initial,
                new Font(ThemeManager.ButtonFont.FontFamily, Math.Max(9, size / 3.2f), FontStyle.Bold),
                rect,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    public class GlassPanel : Panel
    {
        public GlassPanel()
        {
            DoubleBuffered = true;
            Padding = new Padding(28);
            BackColor = Color.Transparent;
        }

        public int Radius { get; set; } = 28;
        public int FillAlpha { get; set; } = 188;

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            using (var path = RoundedPath(ClientRectangle, Radius))
            {
                Region = new Region(path);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = RoundedPath(rect, Radius))
            using (var fill = new SolidBrush(Color.FromArgb(FillAlpha, ThemeManager.CardColor)))
            using (var gloss = new LinearGradientBrush(rect, Color.FromArgb(54, Color.White), Color.FromArgb(4, Color.White), LinearGradientMode.Vertical))
            using (var border = new Pen(Color.FromArgb(120, ThemeManager.BorderColor), 1.2f))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.FillPath(gloss, path);
                e.Graphics.DrawPath(border, path);
            }
        }

        private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
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
    }

    public class PremiumScrollTrack : Panel
    {
        public PremiumScrollTrack()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(2, 4, Math.Max(1, Width - 4), Math.Max(1, Height - 8));
            using (var path = RoundedPath(rect, 6))
            using (var brush = new LinearGradientBrush(rect,
                       Color.FromArgb(70, ColorTranslator.FromHtml("#FF1F2D")),
                       Color.FromArgb(28, ColorTranslator.FromHtml("#090A0F")),
                       LinearGradientMode.Vertical))
            using (var border = new Pen(Color.FromArgb(70, ThemeManager.BorderColor)))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);
            }
        }

        private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
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
    }

    public class PremiumScrollThumb : Panel
    {
        public PremiumScrollThumb()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
            using (var path = RoundedPath(rect, 5))
            using (var brush = new LinearGradientBrush(rect,
                       ColorTranslator.FromHtml("#FF1F2D"),
                       ColorTranslator.FromHtml("#B20710"),
                       LinearGradientMode.Vertical))
            using (var glow = new Pen(Color.FromArgb(150, ColorTranslator.FromHtml("#FF7A82")), 1))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(glow, path);
            }
        }

        private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
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
    }

    public class KpiCard : SoftPanel
    {
        private readonly Label _titleLabel;
        private readonly Label _valueLabel;
        private readonly Label _subtitleLabel;
        private readonly Label _trendLabel;

        public KpiCard()
        {
            Radius = 20;
            HoverAccent = true;
            ShowGradient = true;
            Padding = new Padding(22, 18, 22, 18);
            Height = 148;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                RowCount = 3,
                ColumnCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

            _titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.BottomLeft,
                BackColor = Color.Transparent,
                Tag = "muted-label"
            };
            layout.Controls.Add(_titleLabel, 0, 0);

            _valueLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeManager.LargeKpiFont,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            layout.Controls.Add(_valueLabel, 0, 1);

            _trendLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeManager.BadgeFont,
                TextAlign = ContentAlignment.TopRight,
                BackColor = Color.Transparent
            };
            layout.Controls.Add(_trendLabel, 1, 1);

            _subtitleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeManager.MicroFont,
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent,
                Tag = "muted-label"
            };
            layout.Controls.Add(_subtitleLabel, 0, 2);

            Controls.Add(layout);
        }

        public string Title { set => _titleLabel.Text = value; }
        public string Value { set => _valueLabel.Text = value; }
        public string Subtitle { set => _subtitleLabel.Text = value; }
        public string Trend
        {
            set
            {
                _trendLabel.Text = value;
                _trendLabel.ForeColor = value?.StartsWith("+", StringComparison.Ordinal) == true
                    ? ThemeManager.SuccessColor
                    : ThemeManager.AccentColor;
            }
        }
    }

    public enum ToastType
    {
        Success,
        Error,
        Warning,
        Info
    }

    public class ToastNotification : Form
    {
        private ToastNotification()
        {
        }

        public static void Show(Form parent, string message, ToastType type = ToastType.Info, int durationMs = 3000)
        {
            var toast = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Width = 320,
                Height = 56,
                TopMost = true,
                ShowInTaskbar = false,
                BackColor = ThemeManager.CurrentTheme == AppTheme.Dark
                    ? ColorTranslator.FromHtml("#1A1F30")
                    : ColorTranslator.FromHtml("#FFFFFF"),
                Opacity = 0
            };

            if (parent != null)
            {
                toast.Location = new Point(parent.Right - toast.Width - 18, parent.Bottom - toast.Height - 18);
            }

            var color = type switch
            {
                ToastType.Success => ThemeManager.SuccessColor,
                ToastType.Error => ColorTranslator.FromHtml("#EF4444"),
                ToastType.Warning => ThemeManager.WarningColor,
                _ => ColorTranslator.FromHtml("#3B82F6")
            };
            var icon = type switch
            {
                ToastType.Success => "✓",
                ToastType.Error => "✕",
                ToastType.Warning => "⚠",
                _ => "ℹ"
            };

            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            panel.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                using (var path = RoundedPathStatic(rect, 12))
                using (var fill = new SolidBrush(toast.BackColor))
                using (var border = new Pen(Color.FromArgb(180, color), 1.5f))
                using (var accentBar = new SolidBrush(color))
                {
                    e.Graphics.FillPath(fill, path);
                    e.Graphics.DrawPath(border, path);
                    e.Graphics.FillRectangle(accentBar, new Rectangle(0, 10, 4, panel.Height - 20));
                }
            };

            var iconLabel = new Label
            {
                Text = icon,
                Width = 40,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = color,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            var msgLabel = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ThemeManager.TextColor,
                Font = ThemeManager.BodyFont,
                BackColor = Color.Transparent,
                Padding = new Padding(4, 0, 8, 0)
            };
            panel.Controls.Add(msgLabel);
            panel.Controls.Add(iconLabel);
            toast.Controls.Add(panel);

            var fadeIn = new Timer { Interval = 20 };
            fadeIn.Tick += (_, __) =>
            {
                toast.Opacity = Math.Min(1, toast.Opacity + 0.1);
                if (toast.Opacity >= 1)
                {
                    fadeIn.Stop();
                }
            };

            var hold = new Timer { Interval = durationMs };
            hold.Tick += (_, __) =>
            {
                hold.Stop();
                var fadeOut = new Timer { Interval = 20 };
                fadeOut.Tick += (_, __) =>
                {
                    toast.Opacity = Math.Max(0, toast.Opacity - 0.08);
                    if (toast.Opacity <= 0)
                    {
                        fadeOut.Stop();
                        toast.Close();
                    }
                };
                fadeOut.Start();
            };

            toast.Shown += (_, __) =>
            {
                fadeIn.Start();
                hold.Start();
            };
            toast.Show(parent);
        }

        private static GraphicsPath RoundedPathStatic(Rectangle bounds, int radius)
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
    }

    public class ToggleSwitch : Control
    {
        private bool _checked;
        private bool _hover;

        public ToggleSwitch()
        {
            Width = 52;
            Height = 28;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        public bool Checked
        {
            get => _checked;
            set
            {
                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CheckedChanged;

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var track = new Rectangle(0, 4, Width - 1, Height - 9);
            var trackColor = _checked
                ? ThemeManager.AccentColor
                : (_hover ? ThemeManager.BorderColor : ColorTranslator.FromHtml("#3A3F52"));

            using (var trackBrush = new SolidBrush(trackColor))
            using (var trackPath = RoundedPath(track, track.Height / 2))
            using (var thumbBrush = new SolidBrush(Color.White))
            {
                e.Graphics.FillPath(trackBrush, trackPath);
                var thumbX = _checked ? Width - 22 : 2;
                var thumbRect = new Rectangle(thumbX, 2, 22, Height - 5);
                e.Graphics.FillEllipse(thumbBrush, thumbRect);
            }
        }

        private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
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
    }

    public class PremiumScrollPanel : UserControl
    {
        private readonly Panel _viewport;
        private readonly Panel _track;
        private readonly Panel _thumb;
        private readonly Timer _scrollTimer;
        private int _offset;
        private int _targetOffset;
        private bool _dragging;
        private int _dragStartY;
        private int _dragStartOffset;

        public PremiumScrollPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;

            _viewport = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Tag = "transparent"
            };
            Content = new Panel
            {
                BackColor = Color.Transparent,
                Tag = "transparent"
            };
            _viewport.Controls.Add(Content);
            Controls.Add(_viewport);

            _track = new PremiumScrollTrack
            {
                Dock = DockStyle.Right,
                Width = 12,
                Padding = new Padding(3),
                BackColor = Color.Transparent
            };
            _thumb = new PremiumScrollThumb
            {
                Width = 6,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            _track.Controls.Add(_thumb);
            Controls.Add(_track);

            _scrollTimer = new Timer { Interval = 15 };
            _scrollTimer.Tick += (_, __) => AnimateScroll();
            _viewport.MouseWheel += (_, e) => ScrollBy(-e.Delta / 3);
            Content.MouseWheel += (_, e) => ScrollBy(-e.Delta / 3);
            _thumb.MouseDown += (_, e) =>
            {
                _dragging = true;
                _scrollTimer.Stop();
                _dragStartY = e.Y;
                _dragStartOffset = _offset;
            };
            _thumb.MouseMove += (_, e) =>
            {
                if (!_dragging)
                {
                    return;
                }

                var maxOffset = GetMaxOffset();
                var maxThumbTop = Math.Max(1, _track.ClientSize.Height - _thumb.Height - 8);
                var delta = e.Y - _dragStartY;
                var next = _dragStartOffset + (int)(delta * (maxOffset / (float)maxThumbTop));
                SetOffset(next, true);
            };
            _thumb.MouseUp += (_, __) => _dragging = false;
            _thumb.MouseLeave += (_, __) => _dragging = false;
            Resize += (_, __) => UpdateLayout();
            ThemeManager.ThemeChanged += UpdateTheme;
        }

        public Panel Content { get; }
        public int MinimumContentHeight { get; set; }

        public void ScrollToTop()
        {
            SetOffset(0, true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.ThemeChanged -= UpdateTheme;
                _scrollTimer.Stop();
                _scrollTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            UpdateLayout();
        }

        public void UpdateLayout()
        {
            if (_viewport == null || _track == null || _thumb == null || Content == null)
            {
                return;
            }

            var width = Math.Max(1, _viewport.ClientSize.Width);
            var height = Math.Max(_viewport.ClientSize.Height, MinimumContentHeight);
            Content.SetBounds(0, -_offset, width, height);
            foreach (Control child in Content.Controls)
            {
                if (child.Dock == DockStyle.Fill)
                {
                    child.Size = Content.ClientSize;
                }

                AttachMouseWheel(child);
            }

            SetOffset(_offset, true);
            UpdateTheme();
        }

        private void ScrollBy(int delta)
        {
            SetOffset(_targetOffset + delta, false);
        }

        private void AttachMouseWheel(Control control)
        {
            control.MouseWheel -= ChildMouseWheel;
            control.MouseWheel += ChildMouseWheel;
            foreach (Control child in control.Controls)
            {
                AttachMouseWheel(child);
            }
        }

        private void ChildMouseWheel(object sender, MouseEventArgs e)
        {
            ScrollBy(-e.Delta / 3);
        }

        private void SetOffset(int value, bool immediate)
        {
            if (_track == null || _thumb == null || _viewport == null || Content == null)
            {
                return;
            }

            var maxOffset = GetMaxOffset();
            _targetOffset = Math.Max(0, Math.Min(value, maxOffset));
            if (immediate)
            {
                _offset = _targetOffset;
                _scrollTimer?.Stop();
            }
            else if (_scrollTimer != null && !_scrollTimer.Enabled)
            {
                _scrollTimer.Start();
            }

            ApplyOffset();
        }

        private void ApplyOffset()
        {
            Content.Top = -_offset;

            var maxOffset = GetMaxOffset();
            var needsScroll = maxOffset > 0;
            _track.Visible = needsScroll;
            if (!needsScroll)
            {
                _thumb.Visible = false;
                return;
            }

            _thumb.Visible = true;
            var trackHeight = Math.Max(1, _track.ClientSize.Height - 8);
            var thumbHeight = Math.Max(44, (int)(trackHeight * (_viewport.ClientSize.Height / (float)Math.Max(_viewport.ClientSize.Height, Content.Height))));
            var maxThumbTop = Math.Max(1, trackHeight - thumbHeight);
            var top = 4 + (int)(maxThumbTop * (_offset / (float)maxOffset));
            _thumb.SetBounds(3, top, 6, thumbHeight);
            using (var path = RoundedPath(new Rectangle(0, 0, _thumb.Width, _thumb.Height), 4))
            {
                _thumb.Region = new Region(path);
            }
        }

        private void AnimateScroll()
        {
            var distance = _targetOffset - _offset;
            if (Math.Abs(distance) <= 1)
            {
                _offset = _targetOffset;
                _scrollTimer.Stop();
                ApplyOffset();
                return;
            }

            _offset += Math.Sign(distance) * Math.Max(1, Math.Abs(distance) / 5);
            ApplyOffset();
        }

        private int GetMaxOffset()
        {
            return Math.Max(0, Content.Height - _viewport.ClientSize.Height);
        }

        private void UpdateTheme()
        {
            _track.BackColor = Color.Transparent;
            _thumb.BackColor = Color.Transparent;
            _track.Invalidate();
            _thumb.Invalidate();
        }

        private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
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
    }
}
