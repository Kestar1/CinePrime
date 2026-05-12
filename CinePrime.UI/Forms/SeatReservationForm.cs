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
        private readonly ComboBox _scheduleComboBox;
        private readonly TextBox _nameTextBox;
        private readonly TextBox _phoneTextBox;
        private readonly TextBox _emailTextBox;
        private readonly FlowLayoutPanel _seatPanel;
        private readonly Label _summaryLabel;
        private readonly List<(int Row, int Seat)> _selectedSeats = new List<(int Row, int Seat)>();

        public SeatReservationForm(ServiceRegistry services)
        {
            _services = services;
            Text = "Seat Reservation";
            Width = 900;
            Height = 680;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(22);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Tag = "card", RowCount = 4, ColumnCount = 1, Padding = new Padding(24) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2 };
            for (var i = 0; i < 4; i++) form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            form.Controls.Add(new Label { Text = "Programare" }, 0, 0);
            _scheduleComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _scheduleComboBox.DataSource = _services.ScheduleService.GetAll();
            _scheduleComboBox.DisplayMember = "StartTime";
            _scheduleComboBox.ValueMember = "Id";
            _scheduleComboBox.SelectedIndexChanged += (_, __) => RenderSeats();
            form.Controls.Add(_scheduleComboBox, 1, 0);
            form.Controls.Add(new Label { Text = "Nume" }, 2, 0);
            _nameTextBox = new TextBox { Dock = DockStyle.Fill };
            form.Controls.Add(_nameTextBox, 3, 0);
            form.Controls.Add(new Label { Text = "Telefon" }, 0, 1);
            _phoneTextBox = new TextBox { Dock = DockStyle.Fill };
            form.Controls.Add(_phoneTextBox, 1, 1);
            form.Controls.Add(new Label { Text = "Email" }, 2, 1);
            _emailTextBox = new TextBox { Dock = DockStyle.Fill };
            form.Controls.Add(_emailTextBox, 3, 1);
            root.Controls.Add(form, 0, 0);

            _summaryLabel = new Label { Dock = DockStyle.Fill, Text = "Selecteaza locurile", Font = ThemeManager.ButtonFont };
            root.Controls.Add(_summaryLabel, 0, 1);

            _seatPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true, Padding = new Padding(12), Tag = "transparent" };
            root.Controls.Add(_seatPanel, 0, 2);

            var reserveButton = new PremiumButton { Text = "Confirma rezervarea", Dock = DockStyle.Right, Width = 220 };
            reserveButton.Click += OnReserveClick;
            root.Controls.Add(reserveButton, 0, 3);

            Controls.Add(root);
            ThemeManager.Bind(this);
            RenderSeats();
        }

        private void RenderSeats()
        {
            _selectedSeats.Clear();
            _seatPanel.Controls.Clear();
            var schedule = _scheduleComboBox.SelectedItem as Schedule;
            if (schedule == null) return;

            var hall = _services.DbContext.Halls.FirstOrDefault(h => h.Id == schedule.HallId);
            if (hall == null) return;

            var reserved = _services.ReservationService.GetReservedSeats(schedule.Id);
            for (var row = 1; row <= hall.RowsCount; row++)
            {
                for (var seat = 1; seat <= hall.SeatsPerRow; seat++)
                {
                    var isReserved = reserved.Any(r => r.SeatRow == row && r.SeatNumber == seat);
                    var button = new Button
                    {
                        Text = $"{row}-{seat}",
                        Width = 58,
                        Height = 36,
                        Margin = new Padding(4),
                        Enabled = !isReserved,
                        BackColor = isReserved ? ColorTranslator.FromHtml("#6B7280") : ColorTranslator.FromHtml("#242938"),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Tag = (row, seat)
                    };
                    button.FlatAppearance.BorderSize = 0;
                    button.Click += OnSeatClick;
                    _seatPanel.Controls.Add(button);
                }
            }

            UpdateSummary();
        }

        private void OnSeatClick(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var seat = ((int row, int seat))button.Tag;
            var current = (seat.row, seat.seat);
            if (_selectedSeats.Contains(current))
            {
                _selectedSeats.Remove(current);
                button.BackColor = ColorTranslator.FromHtml("#242938");
            }
            else
            {
                _selectedSeats.Add(current);
                button.BackColor = ThemeManager.AccentColor;
            }

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            var schedule = _scheduleComboBox.SelectedItem as Schedule;
            var total = schedule == null ? 0 : schedule.TicketPrice * _selectedSeats.Count;
            _summaryLabel.Text = $"Locuri selectate: {_selectedSeats.Count} | Total: {total:0.00} MDL";
        }

        private void OnReserveClick(object sender, EventArgs e)
        {
            var schedule = _scheduleComboBox.SelectedItem as Schedule;
            if (schedule == null) return;

            var result = _services.ReservationService.CreateSeatReservation(
                schedule.Id,
                _nameTextBox.Text,
                _phoneTextBox.Text,
                _emailTextBox.Text,
                _selectedSeats.Select(s => (s.Row, s.Seat)).ToList());
            MessageBox.Show(result.Message, result.Success ? "Succes" : "Eroare");
            if (result.Success)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
