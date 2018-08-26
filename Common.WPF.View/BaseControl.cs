using System.Windows.Controls;

namespace Scar.Common.WPF.View
{
    public class BaseControl : UserControl
    {
        protected BaseControl()
        {
            this.PreventFocusLoss();
        }
    }
}