using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var shift = _pressed ? 1 : 0;

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
                    var fill = _hover ? ThemeManager.BorderColor : ThemeManager.SurfaceColor;
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

            var textColor = Enabled ? ForeColor : ThemeManager.MutedTextColor;
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
        public CinematicBackgroundPanel()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new LinearGradientBrush(ClientRectangle, ColorTranslator.FromHtml("#E50914"), ColorTranslator.FromHtml("#3A0007"), 32f))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            using (var sweep = new LinearGradientBrush(ClientRectangle, Color.FromArgb(130, ColorTranslator.FromHtml("#FF1F2D")), Color.FromArgb(120, ColorTranslator.FromHtml("#090A0F")), 135f))
            using (var red = new SolidBrush(Color.FromArgb(150, ColorTranslator.FromHtml("#FF1F2D"))))
            using (var deepRed = new SolidBrush(Color.FromArgb(125, ColorTranslator.FromHtml("#B20710"))))
            using (var wine = new SolidBrush(Color.FromArgb(132, ColorTranslator.FromHtml("#5A0008"))))
            using (var soft = new SolidBrush(Color.FromArgb(28, Color.White)))
            using (var vignette = new LinearGradientBrush(ClientRectangle, Color.FromArgb(10, Color.Black), Color.FromArgb(150, Color.Black), 90f))
            using (var linePen = new Pen(Color.FromArgb(28, Color.White), 1))
            {
                g.FillRectangle(sweep, ClientRectangle);
                g.FillEllipse(red, new Rectangle(-Width / 5, Height / 10, Width / 2, Height / 2));
                g.FillEllipse(deepRed, new Rectangle(Width - Width / 3, -Height / 6, Width / 2, Height / 2));
                g.FillEllipse(wine, new Rectangle(Width / 4, Height / 3, Width / 2, Height / 2));
                g.FillEllipse(soft, new Rectangle(Width / 3, Height - Height / 3, Width / 3, Height / 3));
                g.FillRectangle(vignette, ClientRectangle);

                for (var x = -Height; x < Width; x += 90)
                {
                    g.DrawLine(linePen, x, Height, x + Height, 0);
                }
            }
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
