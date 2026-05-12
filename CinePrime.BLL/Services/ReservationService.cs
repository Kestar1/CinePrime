using System.Collections.Generic;
using System.Linq;
using CinePrime.BLL.Models;
using CinePrime.DAL;
using CinePrime.DAL.Entities;

namespace CinePrime.BLL.Services
{
    public class ReservationService
    {
        private readonly DatabaseContext _context;

        public ReservationService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Reservation> GetAll()
        {
            return _context.Reservations.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public (bool Success, string Message) Save(Reservation reservation)
        {
            if (!_context.Schedules.Any(s => s.Id == reservation.ScheduleId))
            {
                return (false, "Programarea selectata nu exista.");
            }

            if (string.IsNullOrWhiteSpace(reservation.CustomerName))
            {
                return (false, "Numele clientului este obligatoriu.");
            }

            if (reservation.TotalAmount < 0)
            {
                return (false, "Suma totala nu poate fi negativa.");
            }

            if (reservation.CreatedBy == 0)
            {
                reservation.CreatedBy = Models.ApplicationSession.CurrentUser?.Id ?? 1;
            }

            if (reservation.Id == 0)
            {
                reservation.Id = _context.NextId(_context.Reservations, r => r.Id);
                _context.Reservations.Add(reservation);
                _context.UpsertReservation(reservation);
                return (true, "Rezervarea a fost adaugata.");
            }

            var current = _context.Reservations.FirstOrDefault(r => r.Id == reservation.Id);
            if (current == null)
            {
                return (false, "Rezervarea nu a fost gasita.");
            }

            current.ScheduleId = reservation.ScheduleId;
            current.CustomerName = reservation.CustomerName;
            current.CustomerPhone = reservation.CustomerPhone;
            current.CustomerEmail = reservation.CustomerEmail;
            current.TotalAmount = reservation.TotalAmount;
            current.Status = reservation.Status;
            current.CreatedBy = reservation.CreatedBy;
            _context.UpsertReservation(current);
            return (true, "Rezervarea a fost actualizata.");
        }

        public (bool Success, string Message) Delete(int id)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                return (false, "Rezervarea nu a fost gasita.");
            }

            _context.ReservationSeats.RemoveAll(s => s.ReservationId == id);
            _context.Sales.RemoveAll(s => s.ReservationId == id);
            _context.Reservations.Remove(reservation);
            _context.DeleteWhere("reservation_seats", "reservation_id", id);
            _context.DeleteWhere("sales", "reservation_id", id);
            _context.DeleteById("reservations", id);
            return (true, "Rezervarea a fost stearsa.");
        }

        public List<ReservationSeat> GetReservedSeats(int scheduleId)
        {
            var reservationIds = _context.Reservations
                .Where(r => r.ScheduleId == scheduleId && r.Status != "cancelled")
                .Select(r => r.Id)
                .ToList();

            return _context.ReservationSeats
                .Where(s => reservationIds.Contains(s.ReservationId))
                .ToList();
        }

        public (bool Success, string Message) CreateSeatReservation(
            int scheduleId,
            string customerName,
            string customerPhone,
            string customerEmail,
            List<(int Row, int Seat)> seats)
        {
            var schedule = _context.Schedules.FirstOrDefault(s => s.Id == scheduleId);
            if (schedule == null)
            {
                return (false, "Programarea nu a fost gasita.");
            }

            if (string.IsNullOrWhiteSpace(customerName))
            {
                return (false, "Numele clientului este obligatoriu.");
            }

            if (seats == null || seats.Count == 0)
            {
                return (false, "Selecteaza cel putin un loc.");
            }

            var reserved = GetReservedSeats(scheduleId);
            if (seats.Any(seat => reserved.Any(r => r.SeatRow == seat.Row && r.SeatNumber == seat.Seat)))
            {
                return (false, "Unul dintre locurile selectate este deja rezervat.");
            }

            var reservation = new Reservation
            {
                Id = _context.NextId(_context.Reservations, r => r.Id),
                ScheduleId = scheduleId,
                CustomerName = customerName.Trim(),
                CustomerPhone = customerPhone?.Trim() ?? string.Empty,
                CustomerEmail = customerEmail?.Trim() ?? string.Empty,
                TotalAmount = schedule.TicketPrice * seats.Count,
                Status = "confirmed",
                CreatedBy = ApplicationSession.CurrentUser?.Id ?? 1,
                CreatedAt = System.DateTime.Now
            };
            _context.Reservations.Add(reservation);
            _context.UpsertReservation(reservation);

            foreach (var seat in seats)
            {
                var reservationSeat = new ReservationSeat
                {
                    Id = _context.NextId(_context.ReservationSeats, s => s.Id),
                    ReservationId = reservation.Id,
                    SeatRow = seat.Row,
                    SeatNumber = seat.Seat,
                    SeatType = "standard"
                };
                _context.ReservationSeats.Add(reservationSeat);
                _context.UpsertReservationSeat(reservationSeat);
            }

            var sale = new Sale
            {
                Id = _context.NextId(_context.Sales, s => s.Id),
                SaleType = "tickets",
                ReservationId = reservation.Id,
                TotalAmount = reservation.TotalAmount,
                PaymentMethod = "cash",
                CreatedBy = reservation.CreatedBy,
                CreatedAt = System.DateTime.Now
            };
            _context.Sales.Add(sale);
            _context.UpsertSale(sale);

            return (true, $"Rezervare creata pentru {seats.Count} locuri. Total: {reservation.TotalAmount:0.00} MDL.");
        }
    }
}
