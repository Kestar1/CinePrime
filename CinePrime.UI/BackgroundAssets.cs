using System;
using System.Drawing;
using System.IO;

namespace CinePrime.UI
{
    internal static class BackgroundAssets
    {
        private static Image _authBackground;

        public static Image GetAuthBackground()
        {
            if (_authBackground != null)
            {
                return _authBackground;
            }

            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Resources", "cinema-auth-background.png"),
                Path.Combine(AppContext.BaseDirectory, "cinema-auth-background.png"),
                Path.Combine(Environment.CurrentDirectory, "CinePrime.UI", "Resources", "cinema-auth-background.png"),
                Path.Combine(Environment.CurrentDirectory, "Resources", "cinema-auth-background.png")
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        _authBackground = Image.FromStream(stream);
                    }

                    return _authBackground;
                }
            }

            return null;
        }
    }
}
