using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinePrime.UI
{
    public class SoftPanel : Panel
    {
        private bool _hover;

        public SoftPanel()
        {
            Tag = "card";
            DoubleBuffered = true;
            Padding = new Padding(22);
            Margin = new Padding(12);
            Cursor = Cursors.Default;
        }

        public int Radius { get; set; } = 18;
        public bool HoverAccent { get; set; } = true;

        protected override void OnMouseEnter(System.EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnResize(System.EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Region = new Region(CreatePath(ClientRectangle, Radius));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var border = _hover && HoverAccent ? ThemeManager.AccentColor : ThemeManager.BorderColor;
            using (var pen = new Pen(border, _hover && HoverAccent ? 1.5f : 1f))
            using (var path = CreatePath(new Rectangle(0, 0, Width - 1, Height - 1), Radius))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        private static GraphicsPath CreatePath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
