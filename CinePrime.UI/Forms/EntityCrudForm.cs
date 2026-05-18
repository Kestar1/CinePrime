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
        private TextBox _searchTextBox;
        private ComboBox _statusFilterComboBox;
        private ComboBox _secondaryFilterComboBox;
        private readonly PropertyInfo _statusProperty;
        private readonly PropertyInfo _secondaryFilterProperty;
        private Label _validationLabel;
        private KpiCard _totalCard;
        private KpiCard _visibleCard;
        private KpiCard _statusCard;
        private int _selectedId;
        private bool _updatingFilters;

        protected readonly FlowLayoutPanel Toolbar;

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
            _statusProperty = typeof(T).GetProperty("Status");
            _secondaryFilterProperty = typeof(T).GetProperty("Category") ?? typeof(T).GetProperty("Role");

            Text = title;
            Width = 1260;
            Height = 860;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1040, 760);
            Padding = new Padding(24);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 390));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 360));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 580));

            Toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 10),
                Tag = "transparent"
            };
            Toolbar.Controls.Add(new Label
            {
                Text = title,
                AutoSize = true,
                Font = ThemeManager.SubtitleFont,
                Margin = new Padding(0, 6, 26, 0)
            });

            var newButton = new PremiumButton { Text = "+ New", Width = 120, Height = 40, Margin = new Padding(0, 6, 8, 0) };
            newButton.Click += (_, __) => ClearForm();
            Toolbar.Controls.Add(newButton);

            var exportButton = new PremiumButton { Text = "Export", Variant = PremiumButtonVariant.Secondary, Width = 120, Height = 40, Margin = new Padding(0, 6, 8, 0) };
            exportButton.Click += (_, __) => ExportCsv();
            Toolbar.Controls.Add(exportButton);
            root.Controls.Add(Toolbar, 0, 0);

            root.Controls.Add(BuildOverviewPanel(title), 0, 1);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Margin = new Padding(0, 4, 0, 8)
            };
            _grid.SelectionChanged += (_, __) => LoadSelectedRow();
            _grid.CellFormatting += OnGridCellFormatting;
            root.Controls.Add(_grid, 0, 2);

            var formCard = new SoftPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(22),
                Radius = 20,
                ShowGradient = true,
                HoverAccent = false,
                Margin = new Padding(0, 18, 0, 0)
            };

            var formShell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(0)
            };
            formShell.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            formShell.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            formShell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            formShell.Controls.Add(new Label
            {
                Text = "Editor si validare",
                Font = ThemeManager.ButtonFont,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            _validationLabel = new Label
            {
                Text = "Completeaza campurile si foloseste filtrele pentru a verifica rapid datele.",
                Font = ThemeManager.CaptionFont,
                Tag = "muted-label",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            formShell.Controls.Add(_validationLabel, 0, 1);

            var form = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                Padding = new Padding(0, 14, 0, 0),
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
                    form.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
                    form.RowCount = row + 1;
                }

                form.Controls.Add(new Label
                {
                    Text = GetDisplayName(prop.Name),
                    AutoSize = true,
                    Font = ThemeManager.CaptionFont,
                    Tag = "muted-label"
                }, columnPair, row);
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
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            var saveButton = new PremiumButton { Text = "Save", Dock = DockStyle.Fill, Margin = new Padding(6, 16, 8, 6) };
            saveButton.Click += (_, __) => SaveCurrent();
            form.Controls.Add(saveButton, 2, actionRow);

            var deleteButton = new PremiumButton { Text = "Delete", Variant = PremiumButtonVariant.Secondary, Dock = DockStyle.Fill, Margin = new Padding(8, 16, 6, 6) };
            deleteButton.Click += (_, __) => DeleteCurrent();
            form.Controls.Add(deleteButton, 3, actionRow);

            formShell.Controls.Add(form, 0, 2);
            formCard.Controls.Add(formShell);
            root.Controls.Add(formCard, 0, 3);

            var scroll = new PremiumScrollPanel
            {
                Dock = DockStyle.Fill,
                MinimumContentHeight = 1560
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
            if (backColor.HasValue)
            {
                btn.Variant = PremiumButtonVariant.Danger;
            }

            btn.Click += onClick;
            Toolbar.Controls.Add(btn);
            ThemeManager.ApplyTheme(btn);
        }

        private Control BuildOverviewPanel(string title)
        {
            var wrapper = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Tag = "transparent",
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            wrapper.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
            wrapper.RowStyles.Add(new RowStyle(SizeType.Absolute, 252));

            var stats = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3,
                Tag = "transparent",
                Margin = new Padding(0),
                Padding = new Padding(0, 0, 0, 6)
            };
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            _totalCard = new KpiCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
            _visibleCard = new KpiCard { Dock = DockStyle.Fill, Margin = new Padding(6, 0, 6, 0) };
            _statusCard = new KpiCard { Dock = DockStyle.Fill, Margin = new Padding(12, 0, 0, 0) };
            stats.Controls.Add(_totalCard, 0, 0);
            stats.Controls.Add(_visibleCard, 1, 0);
            stats.Controls.Add(_statusCard, 2, 0);
            wrapper.Controls.Add(stats, 0, 0);

            var filterCard = new SoftPanel
            {
                Dock = DockStyle.Fill,
                Radius = 20,
                ShowGradient = true,
                HoverAccent = false,
                Padding = new Padding(20, 20, 20, 22),
                Margin = new Padding(0, 18, 0, 0)
            };

            var filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Tag = "transparent",
                RowCount = 3,
                ColumnCount = 3,
                Padding = new Padding(0, 0, 0, 14),
                Margin = new Padding(0)
            };
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

            filterLayout.Controls.Add(new Label
            {
                Text = $"Cautare, filtrare si verificare - {title}",
                Dock = DockStyle.Fill,
                Font = ThemeManager.ButtonFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            filterLayout.SetColumnSpan(filterLayout.GetControlFromPosition(0, 0), 3);

            _searchTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Search / filter...",
                Margin = new Padding(0, 16, 0, 4)
            };
            _searchTextBox.MinimumSize = new Size(0, 46);
            _searchTextBox.TextChanged += (_, __) => RefreshGrid();
            filterLayout.Controls.Add(_searchTextBox, 0, 1);
            filterLayout.SetColumnSpan(_searchTextBox, 3);

            _statusFilterComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 16, 12, 0)
            };
            _statusFilterComboBox.MinimumSize = new Size(0, 46);
            _statusFilterComboBox.SelectedIndexChanged += (_, __) =>
            {
                if (!_updatingFilters)
                {
                    RefreshGrid();
                }
            };
            filterLayout.Controls.Add(_statusFilterComboBox, 0, 2);

            _secondaryFilterComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 16, 12, 0)
            };
            _secondaryFilterComboBox.MinimumSize = new Size(0, 46);
            _secondaryFilterComboBox.SelectedIndexChanged += (_, __) =>
            {
                if (!_updatingFilters)
                {
                    RefreshGrid();
                }
            };
            filterLayout.Controls.Add(_secondaryFilterComboBox, 1, 2);

            var clearFiltersButton = new PremiumButton
            {
                Text = "Reset",
                Variant = PremiumButtonVariant.Secondary,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 14, 0, 6),
                MinimumSize = new Size(0, 48)
            };
            clearFiltersButton.Click += (_, __) => ResetFilters();
            filterLayout.Controls.Add(clearFiltersButton, 2, 2);

            filterCard.Controls.Add(filterLayout);
            wrapper.Controls.Add(filterCard, 0, 1);
            return wrapper;
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
            try
            {
                var allItems = _load();
                RefreshFilterOptions(allItems);
                var items = ApplyFilters(allItems);

                _grid.DataSource = null;
                _grid.DataSource = new BindingList<T>(items);
                UpdateSummaryCards(allItems, items);
            }
            catch (Exception ex)
            {
                _validationLabel.Text = "Nu am putut incarca datele. Verifica filtrarea sau conexiunea.";
                _validationLabel.Tag = "accent-text";
                UiFeedback.ShowException(this, ex, "A aparut o problema la incarcarea listei.");
            }
        }

        private void RefreshFilterOptions(List<T> items)
        {
            _updatingFilters = true;
            try
            {
                var statusSelection = _statusFilterComboBox?.SelectedItem?.ToString();
                var secondarySelection = _secondaryFilterComboBox?.SelectedItem?.ToString();

                BindFilterCombo(_statusFilterComboBox, "Toate statusurile", _statusProperty == null
                    ? Array.Empty<string>()
                    : items.Select(item => GetStringValue(item, _statusProperty)).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v).ToArray(),
                    statusSelection);

                var secondaryCaption = _secondaryFilterProperty == null
                    ? "Toate"
                    : $"Toate {GetDisplayName(_secondaryFilterProperty.Name).ToLowerInvariant()}";
                BindFilterCombo(_secondaryFilterComboBox, secondaryCaption, _secondaryFilterProperty == null
                    ? Array.Empty<string>()
                    : items.Select(item => GetStringValue(item, _secondaryFilterProperty)).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v).ToArray(),
                    secondarySelection);

                _statusFilterComboBox.Enabled = _statusProperty != null;
                _secondaryFilterComboBox.Enabled = _secondaryFilterProperty != null;
            }
            finally
            {
                _updatingFilters = false;
            }
        }

        private static void BindFilterCombo(ComboBox combo, string allText, string[] values, string selectedValue)
        {
            if (combo == null)
            {
                return;
            }

            var items = new List<string> { allText };
            items.AddRange(values);
            combo.DataSource = items;
            var resolved = !string.IsNullOrWhiteSpace(selectedValue) && items.Contains(selectedValue) ? selectedValue : allText;
            combo.SelectedItem = resolved;
        }

        private List<T> ApplyFilters(List<T> items)
        {
            var query = _searchTextBox?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(query))
            {
                items = items.Where(item => typeof(T).GetProperties()
                    .Any(p => Convert.ToString(p.GetValue(item), CultureInfo.InvariantCulture)
                        ?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            }

            if (_statusProperty != null && _statusFilterComboBox.Enabled && _statusFilterComboBox.SelectedIndex > 0)
            {
                var selectedStatus = _statusFilterComboBox.SelectedItem?.ToString();
                items = items.Where(item => string.Equals(GetStringValue(item, _statusProperty), selectedStatus, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (_secondaryFilterProperty != null && _secondaryFilterComboBox.Enabled && _secondaryFilterComboBox.SelectedIndex > 0)
            {
                var selectedSecondary = _secondaryFilterComboBox.SelectedItem?.ToString();
                items = items.Where(item => string.Equals(GetStringValue(item, _secondaryFilterProperty), selectedSecondary, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return items;
        }

        private void UpdateSummaryCards(List<T> allItems, List<T> filteredItems)
        {
            _totalCard.Title = "Total inregistrari";
            _totalCard.Value = allItems.Count.ToString();
            _totalCard.Subtitle = "Date disponibile in modul";
            _totalCard.Trend = string.Empty;

            _visibleCard.Title = "Rezultate vizibile";
            _visibleCard.Value = filteredItems.Count.ToString();
            _visibleCard.Subtitle = string.IsNullOrWhiteSpace(_searchTextBox.Text)
                ? "Fara cautare activa"
                : $"Cautare: {_searchTextBox.Text.Trim()}";
            _visibleCard.Trend = filteredItems.Count == allItems.Count ? string.Empty : $"{filteredItems.Count * 100 / Math.Max(1, allItems.Count)}%";

            if (_statusProperty != null)
            {
                var statusGroups = allItems
                    .GroupBy(item => GetStringValue(item, _statusProperty))
                    .OrderByDescending(group => group.Count())
                    .FirstOrDefault();
                _statusCard.Title = "Status dominant";
                _statusCard.Value = statusGroups == null ? "N/A" : GetDisplayName(statusGroups.Key);
                _statusCard.Subtitle = statusGroups == null ? "Nu exista date" : $"{statusGroups.Count()} elemente in aceasta stare";
                _statusCard.Trend = statusGroups == null ? string.Empty : $"+{statusGroups.Count()}";
            }
            else if (_secondaryFilterProperty != null)
            {
                var distinctCount = allItems
                    .Select(item => GetStringValue(item, _secondaryFilterProperty))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
                _statusCard.Title = $"Tipuri de {GetDisplayName(_secondaryFilterProperty.Name).ToLowerInvariant()}";
                _statusCard.Value = distinctCount.ToString();
                _statusCard.Subtitle = "Filtrare rapida disponibila";
                _statusCard.Trend = string.Empty;
            }
            else
            {
                _statusCard.Title = "Validare";
                _statusCard.Value = "Activa";
                _statusCard.Subtitle = "Verificare la salvare si stergere";
                _statusCard.Trend = string.Empty;
            }
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

            _validationLabel.Text = $"Editezi inregistrarea #{_selectedId}. Modificarile sunt validate inainte de salvare.";
            _validationLabel.Tag = "muted-label";
            ThemeManager.ApplyTheme(_validationLabel);
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

            _validationLabel.Text = "Formular pregatit pentru o inregistrare noua.";
            _validationLabel.Tag = "muted-label";
            ThemeManager.ApplyTheme(_validationLabel);
        }

        private void ResetFilters()
        {
            _searchTextBox.Text = string.Empty;
            if (_statusFilterComboBox.Items.Count > 0)
            {
                _statusFilterComboBox.SelectedIndex = 0;
            }

            if (_secondaryFilterComboBox.Items.Count > 0)
            {
                _secondaryFilterComboBox.SelectedIndex = 0;
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
                if (result.Success)
                {
                    _validationLabel.Text = "Datele au trecut validarile si au fost salvate cu succes.";
                    _validationLabel.Tag = "muted-label";
                    ThemeManager.ApplyTheme(_validationLabel);
                    ToastNotification.Show(this, result.Message, ToastType.Success);
                    ClearForm();
                    RefreshGrid();
                }
                else
                {
                    _validationLabel.Text = result.Message;
                    _validationLabel.Tag = "accent-text";
                    ThemeManager.ApplyTheme(_validationLabel);
                    UiFeedback.ShowError(this, result.Message);
                }
            }
            catch (Exception ex)
            {
                _validationLabel.Text = "Datele introduse nu sunt valide. Verifica campurile numerice si valorile obligatorii.";
                _validationLabel.Tag = "accent-text";
                ThemeManager.ApplyTheme(_validationLabel);
                UiFeedback.ShowException(this, ex, "Nu am putut salva inregistrarea.");
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
                UiFeedback.ShowInfo(this, "Selecteaza o inregistrare pentru stergere.");
                return;
            }

            var confirm = MessageBox.Show("Sigur vrei sa stergi inregistrarea selectata?", "Confirmare", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var result = _delete(_selectedId);
                if (result.Success)
                {
                    ToastNotification.Show(this, result.Message, ToastType.Warning);
                    ClearForm();
                    RefreshGrid();
                }
                else
                {
                    _validationLabel.Text = result.Message;
                    _validationLabel.Tag = "accent-text";
                    ThemeManager.ApplyTheme(_validationLabel);
                    UiFeedback.ShowError(this, result.Message);
                }
            }
            catch (Exception ex)
            {
                UiFeedback.ShowException(this, ex, "Nu am putut sterge inregistrarea selectata.");
            }
        }

        private void ExportCsv()
        {
            try
            {
                using (var dialog = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = $"{typeof(T).Name.ToLowerInvariant()}_export.csv" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    var properties = typeof(T).GetProperties().Where(p => p.CanRead).ToList();
                    var source = ApplyFilters(_load());
                    var lines = new List<string> { string.Join(",", properties.Select(p => EscapeCsv(p.Name))) };
                    foreach (var item in source)
                    {
                        lines.Add(string.Join(",", properties.Select(p => EscapeCsv(Convert.ToString(p.GetValue(item), CultureInfo.InvariantCulture) ?? string.Empty))));
                    }

                    File.WriteAllLines(dialog.FileName, lines, Encoding.UTF8);
                    ToastNotification.Show(this, "Exportul filtrat a fost salvat.", ToastType.Success);
                }
            }
            catch (Exception ex)
            {
                UiFeedback.ShowException(this, ex, "Exportul nu a putut fi generat.");
            }
        }

        private static string EscapeCsv(string value)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private void OnGridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name != "Status" || e.Value == null)
            {
                return;
            }

            var status = e.Value.ToString().ToLowerInvariant();
            e.CellStyle.ForeColor = status switch
            {
                "active" or "confirmed" or "scheduled" => ThemeManager.SuccessColor,
                "pending" or "upcoming" => ThemeManager.WarningColor,
                "cancelled" or "inactive" => ColorTranslator.FromHtml("#EF4444"),
                _ => ThemeManager.MutedTextColor
            };
            e.CellStyle.Font = new Font(
                ThemeManager.CaptionFont.FontFamily,
                ThemeManager.CaptionFont.Size,
                FontStyle.Bold);
            e.FormattingApplied = true;
        }

        private static string GetStringValue(T item, PropertyInfo property)
        {
            return Convert.ToString(property?.GetValue(item), CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string GetDisplayName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return string.Empty;
            }

            var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FullName"] = "Full Name",
                ["RowsCount"] = "Rows Count",
                ["SeatsPerRow"] = "Seats Per Row",
                ["HallType"] = "Hall Type",
                ["MovieId"] = "Movie",
                ["HallId"] = "Hall",
                ["TicketPrice"] = "Ticket Price",
                ["VipPrice"] = "VIP Price",
                ["ScheduleId"] = "Schedule",
                ["CustomerName"] = "Customer Name",
                ["CustomerPhone"] = "Customer Phone",
                ["CustomerEmail"] = "Customer Email",
                ["TotalAmount"] = "Total Amount",
                ["CreatedBy"] = "Created By",
                ["StockQuantity"] = "Stock Quantity",
                ["MinStockAlert"] = "Min Stock Alert",
                ["ReleaseYear"] = "Release Year",
                ["DurationMinutes"] = "Duration (min)",
                ["TotalReviews"] = "Total Reviews"
            };
            if (replacements.TryGetValue(rawName, out var mapped))
            {
                return mapped;
            }

            var builder = new StringBuilder();
            foreach (var ch in rawName)
            {
                if (char.IsUpper(ch) && builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
