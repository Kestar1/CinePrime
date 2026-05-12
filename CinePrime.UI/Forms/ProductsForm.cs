using CinePrime.BLL.Models;
using CinePrime.DAL.Entities;
using System.Windows.Forms;

namespace CinePrime.UI.Forms
{
    public class ProductsForm : EntityCrudForm<Product>
    {
        public ProductsForm(ServiceRegistry services)
            : base("Products", services.ProductService.GetAll, services.ProductService.Save, services.ProductService.Delete)
        {
            AddHeaderAction("Quick Sale", (_, __) =>
            {
                using (var form = new QuickSaleForm(services))
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
