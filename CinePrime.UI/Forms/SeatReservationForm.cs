using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CinePrime.BLL.Models;
using CinePrime.DAL.Entities;

namespace CinePrime.UI.Forms
{
    public class SeatReservationForm : Form
    {
        private readonly ServiceRegistry _services;
        private ComboBox _scheduleComboBox;
        private TextBox _nameTextBox;
        private TextBox _phoneTextBox;
        private TextBox _emailTextBox;
        private Label _summaryLabel;
        private Label _scheduleDetailsLabel;
        private Label _selectionHintLabel;
        private Panel _seatHost;
        private TableLayoutPanel _seatGrid;
        private readonly Dictionary<Button, SeatState> _seatStates = new Dictionary<Button, SeatState>();
        private readonly List<(int Row, int Seat)> _selectedSeats = new List<(int Row, int Seat)>();

        public SeatReservationForm(ServiceRegistry services)
        {
            _services = services;
            Text = "Seat Reservation";
            Width = 1180;
            Height = 820;
            MinimumSize = new Size(1020, 720);
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(20);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Tag = "card",
                Padding = new Padding(22)
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 134));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));

            shell.Controls.Add(BuildHeader(), 0, 0);
            shell.Controls.Add(BuildCustomerArea(), 0, 1);
            shell.Controls.Add(BuildSeatArea(), 0, 2);
            shell.Controls.Add(BuildFooter(), 0, 3);

            Controls.Add(shell);

            LoadSchedules();
            ThemeManager.Bind(this);
            ThemeManager.ThemeChanged += OnThemeChanged;
            FormClosed += (_, __) => ThemeManager.ThemeChanged -= OnThemeChanged;
            RenderSeats();
        }

        private Control BuildHeader()
        {
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 10)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            header.Controls.Add(new Label
            {
                Text = "Rezervare cu locuri",
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.SubtitleFont.FontFamily, 20, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            header.Controls.Add(new Label
            {
                Text = "Alege programarea, completeaza datele clientului si selecteaza locurile libere.",
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                Tag = "muted-label",
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            _summaryLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.ButtonFont.FontFamily, 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Tag = "accent-text"
            };
            header.SetRowSpan(_summaryLabel, 2);
            header.Controls.Add(_summaryLabel, 1, 0);

            return header;
        }

        private Control BuildCustomerArea()
        {
            var customerCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(18, 16, 18, 12),
                Tag = "card"
            };
            customerCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            customerCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            customerCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            customerCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            customerCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            customerCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            customerCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            customerCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            customerCard.Controls.Add(new Label
            {
                Text = "Detalii rezervare",
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.ButtonFont.FontFamily, 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            customerCard.SetColumnSpan(customerCard.GetControlFromPosition(0, 0), 4);

            customerCard.Controls.Add(new Label { Text = "Programare", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            _scheduleComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _scheduleComboBox.SelectedIndexChanged += (_, __) => RenderSeats();
            customerCard.Controls.Add(_scheduleComboBox, 1, 1);

            customerCard.Controls.Add(new Label { Text = "Nume client", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 1);
            _nameTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Ex: Ion Popescu" };
            customerCard.Controls.Add(_nameTextBox, 3, 1);

            customerCard.Controls.Add(new Label { Text = "Telefon", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            _phoneTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Ex: 069000000" };
            customerCard.Controls.Add(_phoneTextBox, 1, 2);

            customerCard.Controls.Add(new Label { Text = "Email", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 2);
            _emailTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Ex: client@cineprime.md" };
            customerCard.Controls.Add(_emailTextBox, 3, 2);

            _scheduleDetailsLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                Tag = "muted-label",
                TextAlign = ContentAlignment.MiddleLeft
            };
            customerCard.SetColumnSpan(_scheduleDetailsLabel, 4);
            customerCard.Controls.Add(_scheduleDetailsLabel, 0, 3);

            return customerCard;
        }

        private Control BuildSeatArea()
        {
            var seatArea = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = new Padding(0, 16, 0, 10)
            };
            seatArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            seatArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            seatArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            seatArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            seatArea.Controls.Add(new Label
            {
                Text = "Selectarea locurilor",
                Dock = DockStyle.Fill,
                Font = new Font(ThemeManager.ButtonFont.FontFamily, 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            seatArea.Controls.Add(BuildLegend(), 0, 1);

            _selectionHintLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeManager.CaptionFont,
                Tag = "muted-label",
                Text = "Apasa pe locurile libere pentru a le adauga in rezervare."
            };
            seatArea.Controls.Add(_selectionHintLabel, 0, 2);

            var seatContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Tag = "card",
                Padding = new Padding(16)
            };
            seatContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            seatContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var screenLabel = new Label
            {
                Text = "ECRAN",
                Dock = DockStyle.Top,
                Font = new Font(ThemeManager.ButtonFont.FontFamily, 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = "accent-text",
                Margin = new Padding(140, 0, 140, 10)
            };
            seatContainer.Controls.Add(screenLabel, 0, 0);

            _seatHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Tag = "transparent",
                Padding = new Padding(6)
            };
            _seatHost.Resize += (_, __) => LayoutSeatGrid();
            _seatGrid = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0),
                Padding = new Padding(0),
                Tag = "transparent"
            };
            _seatHost.Controls.Add(_seatGrid);
            seatContainer.Controls.Add(_seatHost, 0, 1);

            seatArea.Controls.Add(seatContainer, 0, 3);
            return seatArea;
        }

        private Control BuildLegend()
        {
            var legend = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0)
            };

            legend.Controls.Add(CreateLegendChip("Liber", SeatState.Free));
            legend.Controls.Add(CreateLegendChip("Selectat", SeatState.Selected));
            legend.Controls.Add(CreateLegendChip("Rezervat", SeatState.Reserved));

            return legend;
        }

        private Control CreateLegendChip(string text, SeatState state)
        {
            var chip = new Panel
            {
                Width = 170,
                Height = 32,
                Margin = new Padding(0, 0, 12, 0),
                Tag = "transparent"
            };

            var dot = new Panel
            {
                Width = 16,
                Height = 16,
                Left = 0,
                Top = 8,
                BackColor = GetSeatColor(state),
                Tag = $"legend-dot:{state}"
            };
            chip.Controls.Add(dot);

            chip.Controls.Add(new Label
            {
                Text = text,
                AutoSize = false,
                Left = 26,
                Top = 5,
                Width = 130,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = ThemeManager.CaptionFont
            });

            return chip;
        }

        private Control BuildFooter()
        {
            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

            var cancelButton = new PremiumButton
            {
                Text = "Inchide",
                Variant = PremiumButtonVariant.Secondary,
                Dock = DockStyle.Fill,
                Margin = new Padding(12, 10, 10, 0)
            };
            cancelButton.Click += (_, __) => Close();
            footer.Controls.Add(cancelButton, 1, 0);

            var reserveButton = new PremiumButton
            {
                Text = "Confirma rezervarea",
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 10, 0, 0)
            };
            reserveButton.Click += OnReserveClick;
            footer.Controls.Add(reserveButton, 2, 0);

            return footer;
        }

        private void LoadSchedules()
        {
            var options = _services.ScheduleService.GetAll()
                .OrderBy(s => s.StartTime)
                .Select(s => new ScheduleOption(s, GetScheduleText(s)))
                .ToList();

            _scheduleComboBox.DataSource = options;
            _scheduleComboBox.DisplayMember = nameof(ScheduleOption.Display);
            _scheduleComboBox.ValueMember = nameof(ScheduleOption.Id);
        }

        private string GetScheduleText(Schedule schedule)
        {
            var movie = _services.DbContext.Movies.FirstOrDefault(m => m.Id == schedule.MovieId)?.Title ?? "Film necunoscut";
            var hall = _services.DbContext.Halls.FirstOrDefault(h => h.Id == schedule.HallId)?.Name ?? "Sala necunoscuta";
            return $"{schedule.StartTime:dd.MM.yyyy HH:mm} | {movie} | {hall} | {schedule.TicketPrice:0.00} MDL";
        }

        private void RenderSeats()
        {
            _selectedSeats.Clear();
            _seatStates.Clear();
            _seatGrid.Controls.Clear();
            _seatGrid.ColumnStyles.Clear();
            _seatGrid.RowStyles.Clear();

            var option = _scheduleComboBox.SelectedItem as ScheduleOption;
            if (option == null)
            {
                _scheduleDetailsLabel.Text = "Nu exista programari disponibile in acest moment.";
                _selectionHintLabel.Text = "Adauga o programare activa pentru a putea rezerva locuri.";
                _summaryLabel.Text = "0 locuri selectate | 0.00 MDL";
                AddEmptySeatMessage("Nu exista programari disponibile.");
                return;
            }

            var schedule = option.Schedule;
            var hall = _services.DbContext.Halls.FirstOrDefault(h => h.Id == schedule.HallId);
            if (hall == null)
            {
                _scheduleDetailsLabel.Text = "Programarea exista, dar sala asociata nu a fost gasita.";
                _selectionHintLabel.Text = "Verifica datele din modulul Halls.";
                _summaryLabel.Text = "0 locuri selectate | 0.00 MDL";
                AddEmptySeatMessage("Sala nu a fost gasita pentru programarea selectata.");
                return;
            }

            if (hall.RowsCount <= 0 || hall.SeatsPerRow <= 0)
            {
                _scheduleDetailsLabel.Text = $"Sala {hall.Name} nu are locurile configurate.";
                _selectionHintLabel.Text = "Completeaza RowsCount si SeatsPerRow in Halls pentru a activa rezervarea cu locuri.";
                _summaryLabel.Text = "0 locuri selectate | 0.00 MDL";
                AddEmptySeatMessage("Sala nu are schema de locuri definita.");
                return;
            }

            var movie = _services.DbContext.Movies.FirstOrDefault(m => m.Id == schedule.MovieId)?.Title ?? "Film necunoscut";
            var reserved = _services.ReservationService.GetReservedSeats(schedule.Id);
            _scheduleDetailsLabel.Text = $"{movie} | {hall.Name} | {schedule.StartTime:dddd, dd MMMM yyyy HH:mm} | Bilet: {schedule.TicketPrice:0.00} MDL";
            _selectionHintLabel.Text = $"Locuri totale: {hall.Capacity} | Rezervate: {reserved.Count} | Libere: {Math.Max(0, hall.Capacity - reserved.Count)}";

            _seatGrid.ColumnCount = hall.SeatsPerRow + 1;
            _seatGrid.RowCount = hall.RowsCount + 1;
            _seatGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
            for (var seat = 1; seat <= hall.SeatsPerRow; seat++)
            {
                _seatGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
                _seatGrid.Controls.Add(CreateHeaderLabel(seat.ToString()), seat, 0);
            }

            for (var row = 1; row <= hall.RowsCount; row++)
            {
                _seatGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
                _seatGrid.Controls.Add(CreateHeaderLabel($"R{row}"), 0, row);

                for (var seat = 1; seat <= hall.SeatsPerRow; seat++)
                {
                    var isReserved = reserved.Any(r => r.SeatRow == row && r.SeatNumber == seat);
                    var seatButton = new Button
                    {
                        Text = seat.ToString(),
                        Width = 46,
                        Height = 40,
                        Margin = new Padding(6),
                        FlatStyle = FlatStyle.Flat,
                        Font = ThemeManager.CaptionFont,
                        Cursor = isReserved ? Cursors.No : Cursors.Hand,
                        Enabled = !isReserved,
                        Tag = (row, seat)
                    };
                    seatButton.FlatAppearance.BorderSize = 0;
                    seatButton.Click += OnSeatClick;
                    SetSeatState(seatButton, isReserved ? SeatState.Reserved : SeatState.Free);
                    _seatGrid.Controls.Add(seatButton, seat, row);
                }
            }

            _seatGrid.PerformLayout();
            LayoutSeatGrid();
            UpdateSummary();
        }

        private Label CreateHeaderLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Width = 54,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(ThemeManager.CaptionFont.FontFamily, 9, FontStyle.Bold),
                Tag = "muted-label"
            };
        }

        private void AddEmptySeatMessage(string message)
        {
            var messageLabel = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(760, 0),
                Text = message,
                Font = ThemeManager.ButtonFont,
                ForeColor = ThemeManager.AccentColor,
                Margin = new Padding(12)
            };
            _seatGrid.ColumnCount = 1;
            _seatGrid.RowCount = 1;
            _seatGrid.Controls.Add(messageLabel, 0, 0);
            _seatGrid.PerformLayout();
            LayoutSeatGrid();
        }

        private void OnSeatClick(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var raw = ((int row, int seat))button.Tag;
            var seat = (raw.row, raw.seat);

            if (_selectedSeats.Contains(seat))
            {
                _selectedSeats.Remove(seat);
                SetSeatState(button, SeatState.Free);
            }
            else
            {
                _selectedSeats.Add(seat);
                SetSeatState(button, SeatState.Selected);
            }

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            var schedule = (_scheduleComboBox.SelectedItem as ScheduleOption)?.Schedule;
            var total = schedule == null ? 0 : schedule.TicketPrice * _selectedSeats.Count;
            var seatsText = _selectedSeats.Count == 0
                ? "Niciun loc selectat"
                : string.Join(", ", _selectedSeats.OrderBy(s => s.Row).ThenBy(s => s.Seat).Select(s => $"R{s.Row}-L{s.Seat}"));
            _summaryLabel.Text = $"{_selectedSeats.Count} locuri | {total:0.00} MDL" + (_selectedSeats.Count > 0 ? $" | {seatsText}" : string.Empty);
        }

        private void SetSeatState(Button button, SeatState state)
        {
            _seatStates[button] = state;
            button.BackColor = GetSeatColor(state);
            button.ForeColor = state == SeatState.Free && ThemeManager.CurrentTheme == AppTheme.Light
                ? ThemeManager.TextColor
                : Color.White;
        }

        private Color GetSeatColor(SeatState state)
        {
            if (ThemeManager.CurrentTheme == AppTheme.Light)
            {
                return state switch
                {
                    SeatState.Free => ColorTranslator.FromHtml("#E5EAF5"),
                    SeatState.Selected => ColorTranslator.FromHtml("#D71920"),
                    SeatState.Reserved => ColorTranslator.FromHtml("#8F98AB"),
                    _ => ThemeManager.SurfaceColor
                };
            }

            return state switch
            {
                SeatState.Free => ColorTranslator.FromHtml("#242938"),
                SeatState.Selected => ColorTranslator.FromHtml("#E50914"),
                SeatState.Reserved => ColorTranslator.FromHtml("#50566B"),
                _ => ThemeManager.SurfaceColor
            };
        }

        private void OnThemeChanged()
        {
            foreach (var entry in _seatStates.ToList())
            {
                SetSeatState(entry.Key, entry.Value);
            }

            RefreshLegendColors(Controls);
            LayoutSeatGrid();
        }

        private void LayoutSeatGrid()
        {
            if (_seatHost == null || _seatGrid == null)
            {
                return;
            }

            _seatGrid.PerformLayout();
            var availableWidth = Math.Max(0, _seatHost.ClientSize.Width - _seatHost.Padding.Horizontal - 18);
            var centeredX = Math.Max(_seatHost.Padding.Left, _seatHost.Padding.Left + (availableWidth - _seatGrid.PreferredSize.Width) / 2);
            _seatGrid.Location = new Point(centeredX, _seatHost.Padding.Top);
        }

        private void RefreshLegendColors(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control.Tag is string tag && tag.StartsWith("legend-dot:", StringComparison.Ordinal))
                {
                    if (Enum.TryParse(tag.Substring("legend-dot:".Length), out SeatState state))
                    {
                        control.BackColor = GetSeatColor(state);
                    }
                }

                if (control.HasChildren)
                {
                    RefreshLegendColors(control.Controls);
                }
            }
        }

        private void OnReserveClick(object sender, EventArgs e)
        {
            var schedule = (_scheduleComboBox.SelectedItem as ScheduleOption)?.Schedule;
            if (schedule == null)
            {
                UiFeedback.ShowInfo(this, "Nu exista programare selectata.");
                return;
            }

            try
            {
                var result = _services.ReservationService.CreateSeatReservation(
                    schedule.Id,
                    _nameTextBox.Text,
                    _phoneTextBox.Text,
                    _emailTextBox.Text,
                    _selectedSeats.Select(s => (s.Row, s.Seat)).ToList());
                if (result.Success)
                {
                    ToastNotification.Show(this, result.Message, ToastType.Success, 3600);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                UiFeedback.ShowError(this, result.Message);
            }
            catch (Exception ex)
            {
                UiFeedback.ShowException(this, ex, "Rezervarea nu a putut fi finalizata.");
            }
        }

        private sealed class ScheduleOption
        {
            public ScheduleOption(Schedule schedule, string display)
            {
                Schedule = schedule;
                Display = display;
            }

            public Schedule Schedule { get; }
            public int Id => Schedule.Id;
            public string Display { get; }
        }

        private enum SeatState
        {
            Free,
            Selected,
            Reserved
        }
    }
}
