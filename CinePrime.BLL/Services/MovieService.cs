using System.Collections.Generic;
using System.Linq;
using CinePrime.DAL;
using CinePrime.DAL.Entities;

namespace CinePrime.BLL.Services
{
    public class MovieService
    {
        private readonly DatabaseContext _context;

        public MovieService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Movie> GetAll()
        {
            return _context.Movies.OrderBy(m => m.Title).ToList();
        }

        public (bool Success, string Message) Save(Movie movie)
        {
            if (string.IsNullOrWhiteSpace(movie.Title))
            {
                return (false, "Titlul filmului este obligatoriu.");
            }

            if (movie.DurationMinutes <= 0)
            {
                return (false, "Durata trebuie sa fie mai mare decat 0.");
            }

            if (movie.Id == 0)
            {
                movie.Id = _context.NextId(_context.Movies, m => m.Id);
                _context.Movies.Add(movie);
                _context.UpsertMovie(movie);
                return (true, "Filmul a fost adaugat.");
            }

            var current = _context.Movies.FirstOrDefault(m => m.Id == movie.Id);
            if (current == null)
            {
                return (false, "Filmul nu a fost gasit.");
            }

            current.Title = movie.Title;
            current.Genre = movie.Genre;
            current.DurationMinutes = movie.DurationMinutes;
            current.Director = movie.Director;
            current.ReleaseYear = movie.ReleaseYear;
            current.Description = movie.Description;
            current.PosterUrl = movie.PosterUrl;
            current.Rating = movie.Rating;
            current.TotalReviews = movie.TotalReviews;
            current.Status = movie.Status;
            _context.UpsertMovie(current);
            return (true, "Filmul a fost actualizat.");
        }

        public (bool Success, string Message) Delete(int id)
        {
            if (_context.Schedules.Any(s => s.MovieId == id))
            {
                return (false, "Filmul nu poate fi sters deoarece are programari.");
            }

            var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
            {
                return (false, "Filmul nu a fost gasit.");
            }

            _context.Movies.Remove(movie);
            _context.DeleteById("movies", id);
            return (true, "Filmul a fost sters.");
        }
    }
}
