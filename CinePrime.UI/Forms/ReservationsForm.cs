using CinePrime.BLL.Models;
using CinePrime.DAL.Entities;
using System.Windows.Forms;

namespace CinePrime.UI.Forms
{
    public class ReservationsForm : EntityCrudForm<Reservation>
    {
        public ReservationsForm(ServiceRegistry services)
            : base("Reservations", services.ReservationService.GetAll, services.ReservationService.Save, services.ReservationService.Delete)
        {
            AddHeaderAction("Rezervare cu locuri", (_, __) =>
            {
                using (var form = new SeatReservationForm(services))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Close();
                    }
                }
            });
        }
    }
}
