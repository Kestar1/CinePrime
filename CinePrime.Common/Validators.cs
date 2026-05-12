using System.Text.RegularExpressions;

namespace CinePrime.Common
{
    public static class Validators
    {
        public static bool IsEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return Regex.IsMatch(value.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool HasMinimumLength(string value, int length)
        {
            return !string.IsNullOrEmpty(value) && value.Trim().Length >= length;
        }
    }
}
