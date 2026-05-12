using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace CinePrime.UI.Forms
{
    public class EntityCrudForm<T> : Form where T : class, new()
    {
        private readonly Func<List<T>> _load;
        private readonly Func<T, (bool Success, string Message)> _save;
        private readonly Func<int, (bool Success, string Message)> _delete;
        private readonly List<PropertyInfo> _editableProperties;
        private readonly Dictionary<string, Control> _editors = new Dictionary<string, Control>();
        private readonly DataGridView _grid;
        protected readonly FlowLayoutPanel Toolbar;
        private readonly TextBox _searchTextBox;
        private int _selectedId;

        public EntityCrudForm(
            string title,
            Func<List<T>> load,
            Func<T, (bool Success, string Message)> save,
            Func<int, (bool Success, string Message)> delete)
        {
            _load = load;
            _save = save;
            _delete = delete;
            _editableProperties = typeof(T).GetProperties()
                .Where(p => p.CanRead && p.CanWrite && p.Name != "Id" && p.Name != "CreatedAt")
                .ToList();

            Text = title;
            Width = 1220;
            Height = 760;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(980, 680);
            Padding = new Padding(24);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 320));

            Toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 10),
                Tag = "transparent"
            };
            Toolbar.Controls.Add(new Label { Text = title, AutoSize = true, Font = ThemeManager.SubtitleFont, Margin = new Padding(0, 6, 26, 0) });
            
            _searchTextBox = new TextBox { Width = 300, Height = 42, PlaceholderText = "Search / filter...", Margin = new Padding(0, 4, 14, 0) };
            _searchTextBox.TextChanged += (_, __) => RefreshGrid();
            Toolbar.Controls.Add(_searchTextBox);

            var newButton = new PremiumButton { Text = "New", Width = 110, Height = 42, Margin = new Padding(0, 4, 8, 0) };
            newButton.Click += (_, __) => ClearForm();
            Toolbar.Controls.Add(newButton);

            var exportButton = new PremiumButton { Text = "Export", Variant = PremiumButtonVariant.Secondary, Width = 110, Height = 42, Margin = new Padding(0, 4, 8, 0) };
            exportButton.Click += (_, __) => ExportCsv();
            Toolbar.Controls.Add(exportButton);
            
            root.Controls.Add(Toolbar, 0, 0);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _grid.SelectionChanged += (_, __) => LoadSelectedRow();
            root.Controls.Add(_grid, 0, 1);

            var form = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "card",
                Padding = new Padding(18),
                ColumnCount = 4,
                AutoScroll = true
            };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            var row = 0;
            for (var i = 0; i < _editableProperties.Count; i++)
            {
                var prop = _editableProperties[i];
                var columnPair = i % 2 == 0 ? 0 : 2;
                if (columnPair == 0)
                {
                    form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
                    form.RowCount = row + 1;
                }

                form.Controls.Add(new Label { Text = prop.Name, AutoSize = true, Font = ThemeManager.CaptionFont }, columnPair, row);
                var editor = CreateEditor(prop);
                _editors[prop.Name] = editor;
                form.Controls.Add(editor, columnPair + 1, row);

                if (columnPair == 2)
                {
                    row++;
                }
            }

            var actionRow = row;
            form.RowCount = actionRow + 1;
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            var saveButton = new PremiumButton { Text = "Save", Dock = DockStyle.Fill, Margin = new Padding(6, 10, 8, 0) };
            saveButton.Click += (_, __) => SaveCurrent();
            form.Controls.Add(saveButton, 2, actionRow);

            var deleteButton = new PremiumButton { Text = "Delete", Variant = PremiumButtonVariant.Secondary, Dock = DockStyle.Fill, Margin = new Padding(8, 10, 6, 0) };
            deleteButton.Click += (_, __) => DeleteCurrent();
            form.Controls.Add(deleteButton, 3, actionRow);

            root.Controls.Add(form, 0, 2);
            var scroll = new PremiumScrollPanel
            {
                Dock = DockStyle.Fill,
                MinimumContentHeight = 820
            };
            scroll.Content.Controls.Add(root);
            Controls.Add(scroll);
            ThemeManager.Bind(this);
            RefreshGrid();
        }

        protected void AddHeaderAction(string text, EventHandler onClick, Color? backColor = null)
        {
            var btn = new PremiumButton
            {
                Text = text,
                AutoSize = true,
                Padding = new Padding(12, 0, 12, 0),
                Height = 42,
                Margin = new Padding(0, 4, 8, 0)
            };
            if (backColor.HasValue) btn.Variant = PremiumButtonVariant.Danger;
            btn.Click += onClick;
            Toolbar.Controls.Add(btn);
            ThemeManager.ApplyTheme(btn);
        }

        private static Control CreateEditor(PropertyInfo prop)
        {
            if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
            {
                return new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm", Width = 220 };
            }

            return new TextBox { Dock = DockStyle.Fill };
        }

        private void RefreshGrid()
        {
            var items = _load();
            var query = _searchTextBox?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(query))
            {
                items = items.Where(item => typeof(T).GetProperties()
                    .Any(p => Convert.ToString(p.GetValue(item), CultureInfo.InvariantCulture)
                        ?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            }

            _grid.DataSource = null;
            _grid.DataSource = new BindingList<T>(items);
        }

        private void LoadSelectedRow()
        {
            if (_grid.CurrentRow?.DataBoundItem is not T item)
            {
                return;
            }

            _selectedId = Convert.ToInt32(typeof(T).GetProperty("Id")?.GetValue(item) ?? 0, CultureInfo.InvariantCulture);
            foreach (var prop in _editableProperties)
            {
                var value = prop.GetValue(item);
                if (_editors[prop.Name] is DateTimePicker picker)
                {
                    picker.Value = value is DateTime date ? date : DateTime.Now;
                }
                else
                {
                    _editors[prop.Name].Text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
                }
            }
        }

        private void ClearForm()
        {
            _selectedId = 0;
            foreach (var editor in _editors.Values)
            {
                if (editor is DateTimePicker picker)
                {
                    picker.Value = DateTime.Now;
                }
                else
                {
                    editor.Text = string.Empty;
                }
            }
        }

        private void SaveCurrent()
        {
            try
            {
                var item = new T();
                typeof(T).GetProperty("Id")?.SetValue(item, _selectedId);
                foreach (var prop in _editableProperties)
                {
                    prop.SetValue(item, ReadValue(prop, _editors[prop.Name]));
                }

                var result = _save(item);
                MessageBox.Show(result.Message, result.Success ? "Succes" : "Eroare");
                if (result.Success)
                {
                    ClearForm();
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Date invalide: " + ex.Message, "Eroare");
            }
        }

        private static object ReadValue(PropertyInfo prop, Control editor)
        {
            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (targetType == typeof(DateTime) && editor is DateTimePicker picker)
            {
                return picker.Value;
            }

            var text = editor.Text.Trim();
            if (targetType == typeof(string))
            {
                return text;
            }

            if (string.IsNullOrWhiteSpace(text) && Nullable.GetUnderlyingType(prop.PropertyType) != null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return targetType == typeof(string) ? string.Empty : Activator.CreateInstance(targetType);
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, text, true);
            }

            return Convert.ChangeType(text, targetType, CultureInfo.InvariantCulture);
        }

        private void DeleteCurrent()
        {
            if (_selectedId == 0)
            {
                MessageBox.Show("Selecteaza o inregistrare pentru stergere.", "Info");
                return;
            }

            var confirm = MessageBox.Show("Sigur vrei sa stergi inregistrarea selectata?", "Confirmare", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            var result = _delete(_selectedId);
            MessageBox.Show(result.Message, result.Success ? "Succes" : "Eroare");
            if (result.Success)
            {
                ClearForm();
                RefreshGrid();
            }
        }

        private void ExportCsv()
        {
            using (var dialog = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = $"{typeof(T).Name.ToLowerInvariant()}_export.csv" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var properties = typeof(T).GetProperties().Where(p => p.CanRead).ToList();
                var lines = new List<string> { string.Join(",", properties.Select(p => EscapeCsv(p.Name))) };
                foreach (var item in _load())
                {
                    lines.Add(string.Join(",", properties.Select(p => EscapeCsv(Convert.ToString(p.GetValue(item), CultureInfo.InvariantCulture) ?? string.Empty))));
                }

                File.WriteAllLines(dialog.FileName, lines, Encoding.UTF8);
                MessageBox.Show("Exportul a fost salvat.", "Succes");
            }
        }

        private static string EscapeCsv(string value)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
