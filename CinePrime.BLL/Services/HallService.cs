using System.Collections.Generic;
using System.Linq;
using CinePrime.DAL;
using CinePrime.DAL.Entities;

namespace CinePrime.BLL.Services
{
    public class HallService
    {
        private readonly DatabaseContext _context;

        public HallService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Hall> GetAll()
        {
            return _context.Halls.OrderBy(h => h.Name).ToList();
        }

        public (bool Success, string Message) Save(Hall hall)
        {
            if (string.IsNullOrWhiteSpace(hall.Name))
            {
                return (false, "Denumirea salii este obligatorie.");
            }

            if (hall.RowsCount <= 0 || hall.SeatsPerRow <= 0)
            {
                return (false, "Randurile si locurile pe rand trebuie sa fie pozitive.");
            }

            hall.Capacity = hall.RowsCount * hall.SeatsPerRow;

            if (hall.Id == 0)
            {
                hall.Id = _context.NextId(_context.Halls, h => h.Id);
                _context.Halls.Add(hall);
                _context.UpsertHall(hall);
                return (true, "Sala a fost adaugata.");
            }

            var current = _context.Halls.FirstOrDefault(h => h.Id == hall.Id);
            if (current == null)
            {
                return (false, "Sala nu a fost gasita.");
            }

            current.Name = hall.Name;
            current.Capacity = hall.Capacity;
            current.RowsCount = hall.RowsCount;
            current.SeatsPerRow = hall.SeatsPerRow;
            current.HallType = hall.HallType;
            current.Status = hall.Status;
            _context.UpsertHall(current);
            return (true, "Sala a fost actualizata.");
        }

        public (bool Success, string Message) Delete(int id)
        {
            if (_context.Schedules.Any(s => s.HallId == id))
            {
                return (false, "Sala nu poate fi stearsa deoarece are programari.");
            }

            var hall = _context.Halls.FirstOrDefault(h => h.Id == id);
            if (hall == null)
            {
                return (false, "Sala nu a fost gasita.");
            }

            _context.Halls.Remove(hall);
            _context.DeleteById("halls", id);
            return (true, "Sala a fost stearsa.");
        }
    }
}
