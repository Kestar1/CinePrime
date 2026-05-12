using System;
using System.Linq;
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

        public QuickSaleForm(ServiceRegistry services)
        {
            _services = services;
            Text = "Quick Sale";
            Width = 460;
            Height = 320;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(22);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Tag = "card", RowCount = 6, ColumnCount = 2, Padding = new Padding(24) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            for (var i = 0; i < 6; i++)
            {
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 5 ? 54 : 40));
            }

            root.Controls.Add(new Label { Text = "Produs", Dock = DockStyle.Fill }, 0, 0);
            _productComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _productComboBox.DataSource = _services.ProductService.GetAll().Where(p => p.StockQuantity > 0).ToList();
            _productComboBox.DisplayMember = "Name";
            _productComboBox.ValueMember = "Id";
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

            var saleButton = new PremiumButton { Text = "Inregistreaza vanzarea", Dock = DockStyle.Fill };
            saleButton.Click += OnSaleClick;
            root.SetColumnSpan(saleButton, 2);
            root.Controls.Add(saleButton, 0, 5);

            Controls.Add(root);
            ThemeManager.Bind(this);
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            var product = _productComboBox.SelectedItem as Product;
            if (product == null)
            {
                _totalLabel.Text = "0.00 MDL";
                return;
            }

            _quantityInput.Maximum = Math.Max(1, product.StockQuantity);
            _totalLabel.Text = $"{product.Price * _quantityInput.Value:0.00} MDL  |  Stoc: {product.StockQuantity}";
        }

        private void OnSaleClick(object sender, EventArgs e)
        {
            var product = _productComboBox.SelectedItem as Product;
            if (product == null)
            {
                MessageBox.Show("Nu exista produse disponibile.", "Info");
                return;
            }

            var result = _services.ProductService.QuickSale(product.Id, (int)_quantityInput.Value, _paymentComboBox.Text);
            MessageBox.Show(result.Message, result.Success ? "Succes" : "Eroare");
            if (result.Success)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
