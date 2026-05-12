using CinePrime.BLL.Models;
using CinePrime.DAL.Entities;

namespace CinePrime.UI.Forms
{
    public class MoviesForm : EntityCrudForm<Movie>
    {
        public MoviesForm(ServiceRegistry services)
            : base("Movies", services.MovieService.GetAll, services.MovieService.Save, services.MovieService.Delete)
        {
        }
    }
}
