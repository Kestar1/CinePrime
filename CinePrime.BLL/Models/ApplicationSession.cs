namespace CinePrime.BLL.Models
{
    public static class ApplicationSession
    {
        public static SessionUser CurrentUser { get; private set; }

        public static bool IsAuthenticated => CurrentUser != null;

        public static bool IsAdmin =>
            CurrentUser != null && CurrentUser.Role == "admin";

        public static void SignIn(SessionUser user)
        {
            CurrentUser = user;
        }

        public static void SignOut()
        {
            CurrentUser = null;
        }
    }
}
