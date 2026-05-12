using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using CinePrime.DAL.Entities;
using MySqlConnector;

namespace CinePrime.DAL
{
    public class DatabaseContext : IDisposable
    {
        private MySqlConnection _connection;

        public DatabaseContext()
        {
            Reload();
        }

        public List<User> Users { get; } = new List<User>();
        public List<Movie> Movies { get; } = new List<Movie>();
        public List<Hall> Halls { get; } = new List<Hall>();
        public List<Schedule> Schedules { get; } = new List<Schedule>();
        public List<Reservation> Reservations { get; } = new List<Reservation>();
        public List<ReservationSeat> ReservationSeats { get; } = new List<ReservationSeat>();
        public List<Product> Products { get; } = new List<Product>();
        public List<Sale> Sales { get; } = new List<Sale>();
        public List<SaleItem> SaleItems { get; } = new List<SaleItem>();

        public MySqlConnection Connection
        {
            get
            {
                if (_connection == null || _connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken)
                {
                    _connection?.Dispose();
                    _connection = new MySqlConnection(GetConnectionString());
                    _connection.Open();
                }

                return _connection;
            }
        }

        public int NextId<T>(IEnumerable<T> source, Func<T, int> selector)
        {
            return source.Any() ? source.Max(selector) + 1 : 1;
        }

        public void Reload()
        {
            Users.Clear();
            Movies.Clear();
            Halls.Clear();
            Schedules.Clear();
            Reservations.Clear();
            ReservationSeats.Clear();
            Products.Clear();
            Sales.Clear();
            SaleItems.Clear();

            Users.AddRange(Query("SELECT * FROM users", ReadUser));
            Movies.AddRange(Query("SELECT * FROM movies", ReadMovie));
            Halls.AddRange(Query("SELECT * FROM halls", ReadHall));
            Schedules.AddRange(Query("SELECT * FROM schedules", ReadSchedule));
            Reservations.AddRange(Query("SELECT * FROM reservations", ReadReservation));
            ReservationSeats.AddRange(Query("SELECT * FROM reservation_seats", ReadReservationSeat));
            Products.AddRange(Query("SELECT * FROM products", ReadProduct));
            Sales.AddRange(Query("SELECT * FROM sales", ReadSale));
            SaleItems.AddRange(Query("SELECT * FROM sale_items", ReadSaleItem));
        }

        public void UpsertMovie(Movie movie)
        {
            const string sql = @"INSERT INTO movies
                (id, title, genre, duration_minutes, director, release_year, description, poster_url, rating, total_reviews, status, created_at)
                VALUES (@id, @title, @genre, @duration, @director, @year, @description, @poster, @rating, @reviews, @status, @created)
                ON DUPLICATE KEY UPDATE title=@title, genre=@genre, duration_minutes=@duration, director=@director,
                release_year=@year, description=@description, poster_url=@poster, rating=@rating, total_reviews=@reviews, status=@status";
            Execute(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@id", movie.Id);
                cmd.Parameters.AddWithValue("@title", movie.Title);
                cmd.Parameters.AddWithValue("@genre", movie.Genre);
                cmd.Parameters.AddWithValue("@duration", movie.DurationMinutes);
                cmd.Parameters.AddWithValue("@director", movie.Director);
                cmd.Parameters.AddWithValue("@year", movie.ReleaseYear);
                cmd.Parameters.AddWithValue("@description", movie.Description);
                cmd.Parameters.AddWithValue("@poster", movie.PosterUrl);
                cmd.Parameters.AddWithValue("@rating", movie.Rating);
                cmd.Parameters.AddWithValue("@reviews", movie.TotalReviews);
                cmd.Parameters.AddWithValue("@status", movie.Status);
                cmd.Parameters.AddWithValue("@created", movie.CreatedAt);
            });
        }

        public void UpsertHall(Hall hall)
        {
            const string sql = @"INSERT INTO halls (id, name, capacity, rows_count, seats_per_row, hall_type, status)
                VALUES (@id, @name, @capacity, @rows, @seats, @type, @status)
                ON DUPLICATE KEY UPDATE name=@name, capacity=@capacity, rows_count=@rows, seats_per_row=@seats,
                hall_type=@type, status=@status";
            Execute(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@id", hall.Id);
                cmd.Parameters.AddWithValue("@name", hall.Name);
                cmd.Parameters.AddWithValue("@capacity", hall.Capacity);
                cmd.Parameters.AddWithValue("@rows", hall.RowsCount);
                cmd.Parameters.AddWithValue("@seats", hall.SeatsPerRow);
                cmd.Parameters.AddWithValue("@type", hall.HallType);
                cmd.Parameters.AddWithValue("@status", hall.Status);
            });
        }

        public void UpsertSchedule(Schedule schedule)
        {
            const string sql = @"INSERT INTO schedules (id, movie_id, hall_id, start_time, end_time, ticket_price, vip_price, status)
                VALUES (@id, @movie, @hall, @start, @end, @ticket, @vip, @status)
                ON DUPLICATE KEY UPDATE movie_id=@movie, hall_id=@hall, start_time=@start, end_time=@end,
                ticket_price=@ticket, vip_price=@vip, status=@status";
            Execute(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@id", schedule.Id);
                cmd.Parameters.AddWithValue("@movie", schedule.MovieId);
                cmd.Parameters.AddWithValue("@hall", schedule.HallId);
                cmd.Parameters.AddWithValue("@start", schedule.StartTime);
                cmd.Parameters.AddWithValue("@end", schedule.EndTime);
                cmd.Parameters.AddWithValue("@ticket", schedule.TicketPrice);
                cmd.Parameters.AddWithValue("@vip", schedule.VipPrice);
                cmd.Parameters.AddWithValue("@status", schedule.Status);
            });
        }

        public void UpsertProduct(Product product)
        {
            const string sql = @"INSERT INTO products (id, name, category, price, stock_quantity, min_stock_alert, status)
                VALUES (@id, @name, @category, @price, @stock, @min, @status)
                ON DUPLICATE KEY UPDATE name=@name, category=@category, price=@price, stock_quantity=@stock,
                min_stock_alert=@min, status=@status";
            Execute(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@id", product.Id);
                cmd.Parameters.AddWithValue("@name", product.Name);
                cmd.Parameters.AddWithValue("@category", product.Category);
                cmd.Parameters.AddWithValue("@price", product.Price);
                cmd.Parameters.AddWithValue("@stock", product.StockQuantity);
                cmd.Parameters.AddWithValue("@min", product.MinStockAlert);
                cmd.Parameters.AddWithValue("@status", product.Status);
            });
        }

        public void UpsertReservation(Reservation reservation)
        {
            const string sql = @"INSERT INTO reservations
                (id, schedule_id, customer_name, customer_phone, customer_email, total_amount, status, created_by, created_at)
                VALUES (@id, @schedule, @name, @phone, @email, @total, @status, @createdBy, @created)
                ON DUPLICATE KEY UPDATE schedule_id=@schedule, customer_name=@name, customer_phone=@phone,
                customer_email=@email, total_amount=@total, status=@status, created_by=@createdBy";
            Execute(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@id", reservation.Id);
                cmd.Parameters.AddWithValue("@schedule", reservation.ScheduleId);
                cmd.Parameters.AddWithValue("@name", reservation.CustomerName);
                cmd.Parameters.AddWithValue("@phone", reservation.CustomerPhone);
                cmd.Parameters.AddWithValue("@email", reservation.CustomerEmail);
                cmd.Parameters.AddWithValue("@total", reservation.TotalAmount);
                cmd.Parameters.AddWithValue("@status", reservation.Status);
                cmd.Parameters.AddWithValue("@createdBy", reservation.CreatedBy);
                cmd.Parameters.AddWithValue("@created", reservation.CreatedAt);
            });
        }

        public void UpsertReservationSeat(ReservationSeat seat)
        {
            const string sql = @"INSERT INTO reservation_seats (id, reservation_id, seat_row, seat_number, seat_type)
                VALUES (@id, @reservation, @row, @number, @type)
                ON DUPLICATE KEY UPDATE reservation_id=@reservation, seat_row=@row, seat_number=@number, seat_type=@type";
            Execute(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@id", seat.Id);
                cmd.Parameters.AddWithValue("@reservation", seat.ReservationId);
                cmd.Parameters.AddWithValue("@row", seat.SeatRow);
                cmd.Parameters.AddWithValue("@number", seat.SeatNumber);
                cmd.Parameters.AddWithValue("@type", seat.SeatType);
            });
        }

        public void UpsertSale(Sale sale)
        {
            const string sql = @"INSERT INTO sales (id, sale_type, reservation_id, total_amount, payment_method, created_by, created_at)
                VALUES (@id, @type, @reservation, @total, @payment, @createdBy, @created)
                ON DUPLICATE KEY UPDATE sale_type=@type, reservation_id=@reservation, total_amount=@total,
                payment_method=@payment, created_by=@createdBy, created_at=@created";
            Execute(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@id", sale.Id);
                cmd.Parameters.AddWithValue("@type", sale.SaleType);
                cmd.Parameters.AddWithValue("@reservation", sale.ReservationId.HasValue ? sale.ReservationId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@total", sale.TotalAmount);
                cmd.Parameters.AddWithValue("@payment", sale.PaymentMethod);
                cmd.Parameters.AddWithValue("@createdBy", sale.CreatedBy);
                cmd.Parameters.AddWithValue("@created", sale.CreatedAt);
            });
        }

        public void UpsertSaleItem(SaleItem item)
        {
            const string sql = @"INSERT INTO sale_items (id, sale_id, product_id, quantity, unit_price, total_price)
                VALUES (@id, @sale, @product, @quantity, @unit, @total)
                ON DUPLICATE KEY UPDATE sale_id=@sale, product_id=@product, quantity=@quantity, unit_price=@unit, total_price=@total";
            Execute(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@id", item.Id);
                cmd.Parameters.AddWithValue("@sale", item.SaleId);
                cmd.Parameters.AddWithValue("@product", item.ProductId.HasValue ? item.ProductId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                cmd.Parameters.AddWithValue("@unit", item.UnitPrice);
                cmd.Parameters.AddWithValue("@total", item.TotalPrice);
            });
        }

        public void DeleteById(string tableName, int id)
        {
            Execute($"DELETE FROM {tableName} WHERE id=@id", cmd => cmd.Parameters.AddWithValue("@id", id));
        }

        public void DeleteWhere(string tableName, string columnName, int id)
        {
            Execute($"DELETE FROM {tableName} WHERE {columnName}=@id", cmd => cmd.Parameters.AddWithValue("@id", id));
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        private static string GetConnectionString()
        {
            var configured = ConfigurationManager.ConnectionStrings["CinePrimeDB"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }

            throw new InvalidOperationException("Connection string 'CinePrimeDB' nu a fost gasit in App.config.");
        }

        private List<T> Query<T>(string sql, Func<MySqlDataReader, T> map)
        {
            var items = new List<T>();
            using (var cmd = new MySqlCommand(sql, Connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    items.Add(map(reader));
                }
            }

            return items;
        }

        private void Execute(string sql, Action<MySqlCommand> bind)
        {
            using (var cmd = new MySqlCommand(sql, Connection))
            {
                bind?.Invoke(cmd);
                cmd.ExecuteNonQuery();
            }
        }

        private static User ReadUser(MySqlDataReader r) => new User
        {
            Id = GetInt(r, "id"),
            FullName = GetString(r, "full_name"),
            Email = GetString(r, "email"),
            PasswordHash = GetString(r, "password_hash"),
            Role = GetString(r, "role", "operator"),
            Status = GetString(r, "status", "active"),
            ThemePreference = GetString(r, "theme_preference", "dark"),
            LastLogin = GetNullableDate(r, "last_login"),
            CreatedAt = GetDate(r, "created_at", DateTime.Now),
            UpdatedAt = GetDate(r, "updated_at", DateTime.Now)
        };

        private static Movie ReadMovie(MySqlDataReader r) => new Movie
        {
            Id = GetInt(r, "id"),
            Title = GetString(r, "title"),
            Genre = GetString(r, "genre"),
            DurationMinutes = GetInt(r, "duration_minutes"),
            Director = GetString(r, "director"),
            ReleaseYear = GetInt(r, "release_year"),
            Description = GetString(r, "description"),
            PosterUrl = GetString(r, "poster_url"),
            Rating = GetDecimal(r, "rating"),
            TotalReviews = GetInt(r, "total_reviews"),
            Status = GetString(r, "status", "active"),
            CreatedAt = GetDate(r, "created_at", DateTime.Now)
        };

        private static Hall ReadHall(MySqlDataReader r) => new Hall
        {
            Id = GetInt(r, "id"),
            Name = GetString(r, "name"),
            Capacity = GetInt(r, "capacity"),
            RowsCount = GetInt(r, "rows_count"),
            SeatsPerRow = GetInt(r, "seats_per_row"),
            HallType = GetString(r, "hall_type", "standard"),
            Status = GetString(r, "status", "active")
        };

        private static Schedule ReadSchedule(MySqlDataReader r) => new Schedule
        {
            Id = GetInt(r, "id"),
            MovieId = GetInt(r, "movie_id"),
            HallId = GetInt(r, "hall_id"),
            StartTime = GetDate(r, "start_time", DateTime.Now),
            EndTime = GetDate(r, "end_time", DateTime.Now),
            TicketPrice = GetDecimal(r, "ticket_price"),
            VipPrice = GetDecimal(r, "vip_price"),
            Status = GetString(r, "status", "scheduled")
        };

        private static Reservation ReadReservation(MySqlDataReader r) => new Reservation
        {
            Id = GetInt(r, "id"),
            ScheduleId = GetInt(r, "schedule_id"),
            CustomerName = GetString(r, "customer_name"),
            CustomerPhone = GetString(r, "customer_phone"),
            CustomerEmail = GetString(r, "customer_email"),
            TotalAmount = GetDecimal(r, "total_amount"),
            Status = GetString(r, "status", "pending"),
            CreatedBy = GetInt(r, "created_by"),
            CreatedAt = GetDate(r, "created_at", DateTime.Now)
        };

        private static ReservationSeat ReadReservationSeat(MySqlDataReader r) => new ReservationSeat
        {
            Id = GetInt(r, "id"),
            ReservationId = GetInt(r, "reservation_id"),
            SeatRow = GetInt(r, "seat_row"),
            SeatNumber = GetInt(r, "seat_number"),
            SeatType = GetString(r, "seat_type", "standard")
        };

        private static Product ReadProduct(MySqlDataReader r) => new Product
        {
            Id = GetInt(r, "id"),
            Name = GetString(r, "name"),
            Category = GetString(r, "category", "snacks"),
            Price = GetDecimal(r, "price"),
            StockQuantity = GetInt(r, "stock_quantity"),
            MinStockAlert = GetInt(r, "min_stock_alert"),
            Status = GetString(r, "status", "active")
        };

        private static Sale ReadSale(MySqlDataReader r) => new Sale
        {
            Id = GetInt(r, "id"),
            SaleType = GetString(r, "sale_type"),
            ReservationId = GetNullableInt(r, "reservation_id"),
            TotalAmount = GetDecimal(r, "total_amount"),
            PaymentMethod = GetString(r, "payment_method", "cash"),
            CreatedBy = GetInt(r, "created_by"),
            CreatedAt = GetDate(r, "created_at", DateTime.Now)
        };

        private static SaleItem ReadSaleItem(MySqlDataReader r) => new SaleItem
        {
            Id = GetInt(r, "id"),
            SaleId = GetInt(r, "sale_id"),
            ProductId = GetNullableInt(r, "product_id"),
            Quantity = GetInt(r, "quantity"),
            UnitPrice = GetDecimal(r, "unit_price"),
            TotalPrice = GetDecimal(r, "total_price")
        };

        private static int GetInt(MySqlDataReader r, string name) => !HasColumn(r, name) || r.IsDBNull(r.GetOrdinal(name)) ? 0 : Convert.ToInt32(r[name]);
        private static int? GetNullableInt(MySqlDataReader r, string name) => !HasColumn(r, name) || r.IsDBNull(r.GetOrdinal(name)) ? (int?)null : Convert.ToInt32(r[name]);
        private static decimal GetDecimal(MySqlDataReader r, string name) => !HasColumn(r, name) || r.IsDBNull(r.GetOrdinal(name)) ? 0m : Convert.ToDecimal(r[name]);
        private static string GetString(MySqlDataReader r, string name, string fallback = "") => !HasColumn(r, name) || r.IsDBNull(r.GetOrdinal(name)) ? fallback : Convert.ToString(r[name]) ?? fallback;
        private static DateTime GetDate(MySqlDataReader r, string name, DateTime fallback) => !HasColumn(r, name) || r.IsDBNull(r.GetOrdinal(name)) ? fallback : Convert.ToDateTime(r[name]);
        private static DateTime? GetNullableDate(MySqlDataReader r, string name) => !HasColumn(r, name) || r.IsDBNull(r.GetOrdinal(name)) ? (DateTime?)null : Convert.ToDateTime(r[name]);

        private static bool HasColumn(MySqlDataReader r, string name)
        {
            for (var i = 0; i < r.FieldCount; i++)
            {
                if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
