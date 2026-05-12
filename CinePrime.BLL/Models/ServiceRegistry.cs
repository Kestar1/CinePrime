using CinePrime.DAL;
using CinePrime.DAL.Repositories;
using CinePrime.BLL.Services;

namespace CinePrime.BLL.Models
{
    public class ServiceRegistry
    {
        public ServiceRegistry()
        {
            DbContext = new DatabaseContext();
            UserRepository = new UserRepository(DbContext);
            AuthService = new AuthService(UserRepository);
            UserService = new UserService(UserRepository);
            MovieService = new MovieService(DbContext);
            HallService = new HallService(DbContext);
            ScheduleService = new ScheduleService(DbContext);
            ReservationService = new ReservationService(DbContext);
            ProductService = new ProductService(DbContext);
        }

        public DatabaseContext DbContext { get; }
        public UserRepository UserRepository { get; }
        public AuthService AuthService { get; }
        public UserService UserService { get; }
        public MovieService MovieService { get; }
        public HallService HallService { get; }
        public ScheduleService ScheduleService { get; }
        public ReservationService ReservationService { get; }
        public ProductService ProductService { get; }
    }
}
