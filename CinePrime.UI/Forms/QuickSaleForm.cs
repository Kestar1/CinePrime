using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using CinePrime.BLL.Models;
using CinePrime.DAL.Entities;

namespace CinePrime.UI.Forms
{
    public class QuickSaleForm : Form
    {
        private readonly ServiceRegistry _services;
        private readonly ComboBox _productComboBox;
        private readonly NumericUpDown _quantityInput;
        private readonly ComboBox _paymentComboBox;
        private readonly Label _totalLabel;
        private readonly Label _stockListLabel;

        public QuickSaleForm(ServiceRegistry services)
        {
            _services = services;
            Text = "Quick Sale";
            Width = 620;
            Height = 440;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(22);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Tag = "card", RowCount = 7, ColumnCount = 2, Padding = new Padding(26) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

            root.Controls.Add(new Label { Text = "Produs", Dock = DockStyle.Fill }, 0, 0);
            _productComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _productComboBox.DataSource = BuildOptions();
            _productComboBox.DisplayMember = nameof(ProductOption.Display);
            _productComboBox.ValueMember = nameof(ProductOption.Id);
            _productComboBox.SelectedIndexChanged += (_, __) => UpdateTotal();
            root.Controls.Add(_productComboBox, 1, 0);

            root.Controls.Add(new Label { Text = "Cantitate", Dock = DockStyle.Fill }, 0, 1);
            _quantityInput = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 999, Value = 1 };
            _quantityInput.ValueChanged += (_, __) => UpdateTotal();
            root.Controls.Add(_quantityInput, 1, 1);

            root.Controls.Add(new Label { Text = "Plata", Dock = DockStyle.Fill }, 0, 2);
            _paymentComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _paymentComboBox.Items.Add("cash");
            _paymentComboBox.Items.Add("card");
            _paymentComboBox.SelectedIndex = 0;
            root.Controls.Add(_paymentComboBox, 1, 2);

            root.Controls.Add(new Label { Text = "Total", Dock = DockStyle.Fill }, 0, 3);
            _totalLabel = new Label { Dock = DockStyle.Fill, Font = ThemeManager.ButtonFont };
            root.Controls.Add(_totalLabel, 1, 3);
            
            var stockTitle = new Label { Text = "Cantitati disponibile", Dock = DockStyle.Fill, Font = ThemeManager.ButtonFont };
            root.SetColumnSpan(stockTitle, 2);
            root.Controls.Add(stockTitle, 0, 4);

            _stockListLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                TextAlign = System.Drawing.ContentAlignment.TopLeft
            };
            root.SetColumnSpan(_stockListLabel, 2);
            root.Controls.Add(_stockListLabel, 0, 5);

            var saleButton = new PremiumButton { Text = "Inregistreaza vanzarea", Dock = DockStyle.Fill };
            saleButton.Click += OnSaleClick;
            root.SetColumnSpan(saleButton, 2);
            root.Controls.Add(saleButton, 0, 6);

            Controls.Add(root);
            ThemeManager.Bind(this);
            UpdateTotal();
        }

        private List<ProductOption> BuildOptions()
        {
            var products = _services.ProductService.GetAll().Where(p => p.StockQuantity > 0).ToList();
            return products.Select(p => new ProductOption(p)).ToList();
        }

        private void UpdateTotal()
        {
            var product = (_productComboBox.SelectedItem as ProductOption)?.Product;
            if (product == null)
            {
                _totalLabel.Text = "0.00 MDL";
                _stockListLabel.Text = "Nu exista produse disponibile.";
                return;
            }

            _quantityInput.Maximum = Math.Max(1, product.StockQuantity);
            _totalLabel.Text = $"{product.Price * _quantityInput.Value:0.00} MDL  |  Stoc: {product.StockQuantity}";
            _stockListLabel.Text = string.Join(Environment.NewLine, _services.ProductService.GetAll()
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .Select(p => $"{p.Name} ({p.Category}) - {p.StockQuantity} buc. disponibile"));
        }

        private void OnSaleClick(object sender, EventArgs e)
        {
            var product = (_productComboBox.SelectedItem as ProductOption)?.Product;
            if (product == null)
            {
                UiFeedback.ShowInfo(this, "Nu exista produse disponibile.");
                return;
            }

            try
            {
                var result = _services.ProductService.QuickSale(product.Id, (int)_quantityInput.Value, _paymentComboBox.Text);
                if (result.Success)
                {
                    ToastNotification.Show(this, result.Message, ToastType.Success);
                    _productComboBox.DataSource = BuildOptions();
                    UpdateTotal();
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                UiFeedback.ShowError(this, result.Message);
            }
            catch (Exception ex)
            {
                UiFeedback.ShowException(this, ex, "Vanzarea rapida nu a putut fi inregistrata.");
            }
        }

        private sealed class ProductOption
        {
            public ProductOption(Product product)
            {
                Product = product;
            }

            public Product Product { get; }
            public int Id => Product.Id;
            public string Display => $"{Product.Name} | {Product.Price:0.00} MDL | stoc {Product.StockQuantity}";
        }
    }
}
