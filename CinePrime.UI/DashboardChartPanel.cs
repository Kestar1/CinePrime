using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CinePrime.UI
{
    public class DashboardChartPanel : SoftPanel
    {
        public DashboardChartPanel()
        {
            Height = 190;
            Radius = 22;
        }

        public string ChartTitle { get; set; } = string.Empty;
        public decimal[] Values { get; set; } = Array.Empty<decimal>();
        public string[] Labels { get; set; } = Array.Empty<string>();
        public bool DrawLine { get; set; }
        public string ValueSuffix { get; set; } = string.Empty;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var titleBrush = new SolidBrush(ThemeManager.TextColor))
            using (var mutedBrush = new SolidBrush(ThemeManager.MutedTextColor))
            using (var accentBrush = new SolidBrush(ThemeManager.AccentColor))
            using (var linePen = new Pen(ThemeManager.AccentColor, 3))
            using (var gridPen = new Pen(ThemeManager.BorderColor))
            {
                e.Graphics.DrawString(ChartTitle, new Font(ThemeManager.ButtonFont.FontFamily, 12, FontStyle.Bold), titleBrush, 22, 18);

                var valueBandTop = 54;
                var plot = new Rectangle(34, 92, Math.Max(10, Width - 72), Math.Max(10, Height - 142));

                for (var line = 0; line <= 4; line++)
                {
                    var y = plot.Top + line * (plot.Height / 4f);
                    e.Graphics.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                }

                if (Values.Length == 0)
                {
                    e.Graphics.DrawString("No data", ThemeManager.CaptionFont, mutedBrush, plot.Left, plot.Top + 22);
                    return;
                }

                var max = Math.Max(1m, Values.Max());
                if (DrawLine && Values.Length > 1)
                {
                    var points = Values.Select((value, index) =>
                    {
                        var x = plot.Left + index * (plot.Width / (float)(Values.Length - 1));
                        var y = plot.Bottom - (float)(value / max) * plot.Height;
                        return new PointF(x, y);
                    }).ToArray();

                    e.Graphics.DrawLines(linePen, points);
                    foreach (var point in points)
                    {
                        e.Graphics.FillEllipse(accentBrush, point.X - 4, point.Y - 4, 8, 8);
                    }

                    for (var i = 0; i < Values.Length; i++)
                    {
                        var valueLabel = FormatValue(Values[i]);
                        e.Graphics.DrawString(valueLabel, ThemeManager.CaptionFont, mutedBrush, points[i].X - 12, points[i].Y - 24);
                        if (i < Labels.Length)
                        {
                            e.Graphics.DrawString(Labels[i], ThemeManager.CaptionFont, mutedBrush, points[i].X - 8, plot.Bottom + 12);
                        }
                    }
                }
                else
                {
                    var slotWidth = plot.Width / (float)Values.Length;
                    var barWidth = Math.Min(96, Math.Max(34, slotWidth * 0.42f));
                    for (var i = 0; i < Values.Length; i++)
                    {
                        var height = (float)(Values[i] / max) * plot.Height;
                        var x = plot.Left + i * slotWidth + (slotWidth - barWidth) / 2f;
                        var y = plot.Bottom - height;
                        using (var path = RoundedRect(new RectangleF(x, y, barWidth, height), 8))
                        {
                            e.Graphics.FillPath(accentBrush, path);
                        }

                        var valueLabel = FormatValue(Values[i]);
                        var valueSize = e.Graphics.MeasureString(valueLabel, ThemeManager.CaptionFont);
                        var valueX = plot.Left + i * slotWidth + (slotWidth - valueSize.Width) / 2f;
                        e.Graphics.DrawString(valueLabel, ThemeManager.CaptionFont, titleBrush, valueX, valueBandTop);

                        if (i < Labels.Length)
                        {
                            var labelSize = e.Graphics.MeasureString(Labels[i], ThemeManager.CaptionFont);
                            e.Graphics.DrawString(Labels[i], ThemeManager.CaptionFont, mutedBrush, x + (barWidth - labelSize.Width) / 2f, plot.Bottom + 12);
                        }
                    }
                }
            }
        }

        private string FormatValue(decimal value)
        {
            if (value >= 100)
            {
                return value.ToString("0") + ValueSuffix;
            }

            return value.ToString("0.##") + ValueSuffix;
        }

        private static GraphicsPath RoundedRect(RectangleF bounds, float radius)
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
