using System.Windows.Forms;

namespace Demo
{
    public class NoFocusedButton : Button
    {
        protected override bool ShowFocusCues
        {
            get
            {
                return false;
            }
        }
    }
}
