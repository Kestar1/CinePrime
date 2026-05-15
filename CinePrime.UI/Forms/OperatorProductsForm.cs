using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CinePrime.BLL.Models;

namespace CinePrime.UI.Forms
{
    public class OperatorProductsForm : Form
    {
        private readonly ServiceRegistry _services;

        public OperatorProductsForm(ServiceRegistry services)
        {
            _services = services;
            Text = "Products - Operator";
            Width = 720;
            Height = 460;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(620, 420);
            Padding = new Padding(24);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 1,
                Tag = "transparent"
            };

            var card = new SoftPanel
            {
                Dock = DockStyle.Fill,
                Radius = 24,
                Padding = new Padding(34),
                Margin = new Padding(0)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                RowCount = 6,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            layout.Controls.Add(new Label
            {
                Text = "Quick Sale",
                Dock = DockStyle.Fill,
                Font = ThemeManager.SubtitleFont,
                TextAlign = ContentAlignment.BottomLeft
            }, 0, 0);
            layout.Controls.Add(new Label
            {
                Text = "Operatorul poate inregistra vanzari de produse, fara editarea stocului sau a listei de produse.",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = ContentAlignment.TopLeft
            }, 0, 1);

            var summary = new Label
            {
                Text = BuildSummary(),
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.BodyFont.FontFamily, 13, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(summary, 0, 2);

            var quickSaleButton = new PremiumButton
            {
                Text = "Quick Sale",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 0, 8)
            };
            quickSaleButton.Click += (_, __) =>
            {
                using (var form = new QuickSaleForm(_services))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        summary.Text = BuildSummary();
                    }
                }
            };
            layout.Controls.Add(quickSaleButton, 0, 3);

            var closeButton = new PremiumButton
            {
                Text = "Inchide",
                Variant = PremiumButtonVariant.Secondary,
                Dock = DockStyle.Right,
                Width = 160
            };
            closeButton.Click += (_, __) => Close();
            layout.Controls.Add(closeButton, 0, 5);

            card.Controls.Add(layout);
            root.Controls.Add(card, 0, 0);
            Controls.Add(root);
            ThemeManager.Bind(this);
        }

        private string BuildSummary()
        {
            var products = _services.ProductService.GetAll();
            var active = products.Count(p => p.Status != "out_of_stock");
            var lowStock = products.Count(p => p.StockQuantity <= p.MinStockAlert);
            return $"{active} produse disponibile   |   {lowStock} produse cu stoc minim";
        }
    }
}
