using CinePrime.BLL.Models;
using CinePrime.DAL.Entities;

namespace CinePrime.UI.Forms
{
    public class HallsForm : EntityCrudForm<Hall>
    {
        public HallsForm(ServiceRegistry services)
            : base("Halls", services.HallService.GetAll, services.HallService.Save, services.HallService.Delete)
        {
        }
    }
}
