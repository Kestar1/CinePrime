using CinePrime.BLL.Models;
using CinePrime.DAL.Entities;

namespace CinePrime.UI.Forms
{
    public class ScheduleForm : EntityCrudForm<Schedule>
    {
        public ScheduleForm(ServiceRegistry services)
            : base("Schedule", services.ScheduleService.GetAll, services.ScheduleService.Save, services.ScheduleService.Delete)
        {
        }
    }
}
