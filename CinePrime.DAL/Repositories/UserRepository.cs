using System;
using System.Collections.Generic;
using System.Linq;
using CinePrime.DAL.Entities;
using MySqlConnector;

namespace CinePrime.DAL.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseContext _context;

        public UserRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<User> GetAll()
        {
            return _context.Users.OrderBy(u => u.FullName).ToList();
        }

        public User GetById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public User GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        public void Add(User user)
        {
            if (user.Id == 0)
            {
                user.Id = _context.NextId(_context.Users, u => u.Id);
            }

            var columns = GetUserColumns();
            var sql = BuildInsertSql(columns);
            using (var cmd = new MySqlCommand(sql, _context.Connection))
            {
                BindUser(cmd, user, columns);
                cmd.ExecuteNonQuery();
            }

            _context.Users.Add(user);
        }

        public void Update(User user)
        {
            var columns = GetUserColumns();
            var sql = BuildUpdateSql(columns);
            using (var cmd = new MySqlCommand(sql, _context.Connection))
            {
                BindUser(cmd, user, columns);
                cmd.ExecuteNonQuery();
            }

            var current = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (current == null)
            {
                _context.Users.Add(user);
                return;
            }

            current.FullName = user.FullName;
            current.Email = user.Email;
            current.PasswordHash = user.PasswordHash;
            current.Role = user.Role;
            current.Status = user.Status;
            current.ThemePreference = user.ThemePreference;
            current.LastLogin = user.LastLogin;
            current.UpdatedAt = user.UpdatedAt;
        }

        public void Delete(int id)
        {
            _context.DeleteById("users", id);
            _context.Users.RemoveAll(u => u.Id == id);
        }

        private List<string> GetUserColumns()
        {
            var columns = new List<string>();
            using (var cmd = new MySqlCommand("SHOW COLUMNS FROM users", _context.Connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(Convert.ToString(reader["Field"]));
                }
            }

            return columns;
        }

        private static string BuildInsertSql(List<string> columns)
        {
            var supported = columns
                .Where(c => c == "id" || c == "full_name" || c == "email" || c == "password_hash" ||
                            c == "role" || c == "status" || c == "theme_preference" ||
                            c == "last_login" || c == "created_at" || c == "updated_at")
                .ToList();
            var names = string.Join(", ", supported);
            var values = string.Join(", ", supported.Select(c => "@" + c));
            return $"INSERT INTO users ({names}) VALUES ({values})";
        }

        private static string BuildUpdateSql(List<string> columns)
        {
            var supported = columns
                .Where(c => c != "id" &&
                            (c == "full_name" || c == "email" || c == "password_hash" ||
                             c == "role" || c == "status" || c == "theme_preference" ||
                             c == "last_login" || c == "updated_at"))
                .ToList();
            var set = string.Join(", ", supported.Select(c => c + "=@" + c));
            return $"UPDATE users SET {set} WHERE id=@id";
        }

        private static void BindUser(MySqlCommand cmd, User user, List<string> columns)
        {
            AddIfExists(cmd, columns, "id", user.Id);
            AddIfExists(cmd, columns, "full_name", user.FullName);
            AddIfExists(cmd, columns, "email", user.Email);
            AddIfExists(cmd, columns, "password_hash", user.PasswordHash);
            AddIfExists(cmd, columns, "role", user.Role);
            AddIfExists(cmd, columns, "status", user.Status);
            AddIfExists(cmd, columns, "theme_preference", user.ThemePreference);
            AddIfExists(cmd, columns, "last_login", user.LastLogin.HasValue ? user.LastLogin.Value : DBNull.Value);
            AddIfExists(cmd, columns, "created_at", user.CreatedAt);
            AddIfExists(cmd, columns, "updated_at", DateTime.Now);
        }

        private static void AddIfExists(MySqlCommand cmd, List<string> columns, string name, object value)
        {
            if (columns.Any(c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase)))
            {
                cmd.Parameters.AddWithValue("@" + name, value);
            }
        }
    }
}
