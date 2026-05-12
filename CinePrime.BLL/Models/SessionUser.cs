namespace CinePrime.BLL.Models
{
    public class SessionUser
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ThemePreference { get; set; } = "dark";
    }
}
