using System;
using System.Collections.Generic;
using System.Linq;
using CinePrime.DAL;
using CinePrime.DAL.Entities;

namespace CinePrime.BLL.Services
{
    public class ScheduleService
    {
        private readonly DatabaseContext _context;

        public ScheduleService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Schedule> GetAll()
        {
            return _context.Schedules.OrderBy(s => s.StartTime).ToList();
        }

        public (bool Success, string Message) Save(Schedule schedule)
        {
            if (!_context.Movies.Any(m => m.Id == schedule.MovieId))
            {
                return (false, "Filmul selectat nu exista.");
            }

            if (!_context.Halls.Any(h => h.Id == schedule.HallId))
            {
                return (false, "Sala selectata nu exista.");
            }

            if (schedule.EndTime <= schedule.StartTime)
            {
                return (false, "Ora de final trebuie sa fie dupa ora de inceput.");
            }

            if (schedule.TicketPrice <= 0)
            {
                return (false, "Pretul biletului trebuie sa fie pozitiv.");
            }

            var conflict = _context.Schedules.Any(s =>
                s.Id != schedule.Id &&
                s.HallId == schedule.HallId &&
                s.Status != "cancelled" &&
                schedule.StartTime < s.EndTime &&
                schedule.EndTime > s.StartTime);
            if (conflict)
            {
                return (false, "Exista deja o programare in aceeasi sala pentru acest interval.");
            }

            if (schedule.Id == 0)
            {
                schedule.Id = _context.NextId(_context.Schedules, s => s.Id);
                _context.Schedules.Add(schedule);
                _context.UpsertSchedule(schedule);
                return (true, "Programarea a fost adaugata.");
            }

            var current = _context.Schedules.FirstOrDefault(s => s.Id == schedule.Id);
            if (current == null)
            {
                return (false, "Programarea nu a fost gasita.");
            }

            current.MovieId = schedule.MovieId;
            current.HallId = schedule.HallId;
            current.StartTime = schedule.StartTime;
            current.EndTime = schedule.EndTime;
            current.TicketPrice = schedule.TicketPrice;
            current.VipPrice = schedule.VipPrice;
            current.Status = schedule.Status;
            _context.UpsertSchedule(current);
            return (true, "Programarea a fost actualizata.");
        }

        public (bool Success, string Message) Delete(int id)
        {
            if (_context.Reservations.Any(r => r.ScheduleId == id))
            {
                return (false, "Programarea nu poate fi stearsa deoarece are rezervari.");
            }

            var schedule = _context.Schedules.FirstOrDefault(s => s.Id == id);
            if (schedule == null)
            {
                return (false, "Programarea nu a fost gasita.");
            }

            _context.Schedules.Remove(schedule);
            _context.DeleteById("schedules", id);
            return (true, "Programarea a fost stearsa.");
        }
    }
}
