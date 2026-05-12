using System.Collections.Generic;
using System.Linq;
using CinePrime.BLL.Models;
using CinePrime.DAL;
using CinePrime.DAL.Entities;

namespace CinePrime.BLL.Services
{
    public class ProductService
    {
        private readonly DatabaseContext _context;

        public ProductService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Product> GetAll()
        {
            return _context.Products.OrderBy(p => p.Category).ThenBy(p => p.Name).ToList();
        }

        public (bool Success, string Message) Save(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                return (false, "Denumirea produsului este obligatorie.");
            }

            if (product.Price <= 0)
            {
                return (false, "Pretul trebuie sa fie mai mare decat 0.");
            }

            if (product.StockQuantity < 0)
            {
                return (false, "Stocul nu poate fi negativ.");
            }

            if (product.Id == 0)
            {
                product.Id = _context.NextId(_context.Products, p => p.Id);
                _context.Products.Add(product);
                _context.UpsertProduct(product);
                return (true, "Produsul a fost adaugat.");
            }

            var current = _context.Products.FirstOrDefault(p => p.Id == product.Id);
            if (current == null)
            {
                return (false, "Produsul nu a fost gasit.");
            }

            current.Name = product.Name;
            current.Category = product.Category;
            current.Price = product.Price;
            current.StockQuantity = product.StockQuantity;
            current.MinStockAlert = product.MinStockAlert;
            current.Status = product.Status;
            _context.UpsertProduct(current);
            return (true, "Produsul a fost actualizat.");
        }

        public (bool Success, string Message) Delete(int id)
        {
            if (_context.SaleItems.Any(i => i.ProductId == id))
            {
                return (false, "Produsul nu poate fi sters deoarece exista in vanzari.");
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return (false, "Produsul nu a fost gasit.");
            }

            _context.Products.Remove(product);
            _context.DeleteById("products", id);
            return (true, "Produsul a fost sters.");
        }

        public (bool Success, string Message) QuickSale(int productId, int quantity, string paymentMethod)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return (false, "Produsul nu a fost gasit.");
            }

            if (quantity <= 0)
            {
                return (false, "Cantitatea trebuie sa fie mai mare decat 0.");
            }

            if (product.StockQuantity < quantity)
            {
                return (false, "Stoc insuficient pentru vanzare.");
            }

            var total = product.Price * quantity;
            var sale = new Sale
            {
                Id = _context.NextId(_context.Sales, s => s.Id),
                SaleType = "products",
                TotalAmount = total,
                PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "cash" : paymentMethod,
                CreatedBy = ApplicationSession.CurrentUser?.Id ?? 1,
                CreatedAt = System.DateTime.Now
            };
            _context.Sales.Add(sale);
            _context.UpsertSale(sale);
            var saleItem = new SaleItem
            {
                Id = _context.NextId(_context.SaleItems, i => i.Id),
                SaleId = sale.Id,
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = product.Price,
                TotalPrice = total
            };
            _context.SaleItems.Add(saleItem);
            _context.UpsertSaleItem(saleItem);

            product.StockQuantity -= quantity;
            if (product.StockQuantity == 0)
            {
                product.Status = "out_of_stock";
            }
            _context.UpsertProduct(product);

            return (true, $"Vanzare inregistrata: {total:0.00} MDL. Stoc ramas: {product.StockQuantity}.");
        }
    }
}
