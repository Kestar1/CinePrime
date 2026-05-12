using System.Windows.Forms;

namespace CinePrime.UI
{
    public class CardPanel : Panel
    {
        public CardPanel()
        {
            Tag = "card";
            Padding = new Padding(24);
            Margin = new Padding(16);
        }
    }
}
