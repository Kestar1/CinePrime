using System;
using CinePrime.BLL.Models;
using CinePrime.Common;
using CinePrime.DAL.Entities;
using CinePrime.DAL.Repositories;

namespace CinePrime.BLL.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;

        public AuthService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public SessionUser Login(string email, string password)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                return null;
            }

            if (user.Status != Constants.StatusActive)
            {
                return null;
            }

            if (!Helpers.VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            user.LastLogin = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            _userRepository.Update(user);

            return new SessionUser
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                ThemePreference = user.ThemePreference
            };
        }

        public (bool Success, string Message) Register(RegisterUserRequest request)
        {
            if (!Validators.HasMinimumLength(request.FullName, 3))
            {
                return (false, "Numele trebuie sa aiba minim 3 caractere.");
            }

            if (!Validators.IsEmail(request.Email))
            {
                return (false, "Email invalid.");
            }

            if (!Validators.HasMinimumLength(request.Password, 6))
            {
                return (false, "Parola trebuie sa aiba minim 6 caractere.");
            }

            if (_userRepository.GetByEmail(request.Email) != null)
            {
                return (false, "Exista deja un cont cu acest email.");
            }

            var role = request.Role == Constants.RoleAdmin
                ? Constants.RoleAdmin
                : Constants.RoleOperator;

            _userRepository.Add(new User
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                PasswordHash = Helpers.HashPassword(request.Password),
                Role = role,
                Status = Constants.StatusActive,
                ThemePreference = "dark"
            });

            return (true, "Cont creat cu succes.");
        }
    }
}
