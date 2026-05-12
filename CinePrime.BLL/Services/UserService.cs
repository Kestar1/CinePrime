using System.Collections.Generic;
using System.Linq;
using CinePrime.BLL.Models;
using CinePrime.Common;
using CinePrime.DAL.Entities;
using CinePrime.DAL.Repositories;

namespace CinePrime.BLL.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<User> GetUsers()
        {
            return _userRepository.GetAll();
        }

        public (bool Success, string Message) UpdateUser(int id, string fullName, string email, string role, string status)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                return (false, "Utilizatorul nu a fost gasit.");
            }

            if (!Validators.HasMinimumLength(fullName, 3))
            {
                return (false, "Numele trebuie sa aiba minim 3 caractere.");
            }

            if (!Validators.IsEmail(email))
            {
                return (false, "Email invalid.");
            }

            var duplicate = _userRepository.GetAll()
                .Any(u => u.Id != id && string.Equals(u.Email, email, System.StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                return (false, "Exista deja un utilizator cu acest email.");
            }

            user.FullName = fullName.Trim();
            user.Email = email.Trim();
            user.Role = role == Constants.RoleAdmin ? Constants.RoleAdmin : Constants.RoleOperator;
            user.Status = status == Constants.StatusInactive ? Constants.StatusInactive : Constants.StatusActive;
            _userRepository.Update(user);
            return (true, "Utilizatorul a fost actualizat.");
        }

        public (bool Success, string Message) ResetPassword(int id, string password)
        {
            if (!Validators.HasMinimumLength(password, 6))
            {
                return (false, "Parola trebuie sa aiba minim 6 caractere.");
            }

            var user = _userRepository.GetById(id);
            if (user == null)
            {
                return (false, "Utilizatorul nu a fost gasit.");
            }

            user.PasswordHash = Helpers.HashPassword(password);
            _userRepository.Update(user);
            return (true, "Parola a fost resetata.");
        }

        public (bool Success, string Message) DeleteUser(int id)
        {
            var users = _userRepository.GetAll();
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return (false, "Utilizatorul nu a fost gasit.");
            }

            if (users.Count(u => u.Role == Constants.RoleAdmin && u.Status == Constants.StatusActive) <= 1 &&
                user.Role == Constants.RoleAdmin &&
                user.Status == Constants.StatusActive)
            {
                return (false, "Nu poti sterge/dezactiva ultimul administrator activ.");
            }

            user.Status = Constants.StatusInactive;
            _userRepository.Update(user);
            return (true, "Utilizatorul a fost dezactivat.");
        }

        public bool CanManageUsers()
        {
            return ApplicationSession.IsAdmin;
        }

        public bool IsAdmin(string role)
        {
            return role == Constants.RoleAdmin;
        }
    }
}
