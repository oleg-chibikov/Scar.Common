using System.Windows.Controls;

namespace Scar.Common.WPF.View.Core
{
    public class BaseControl : UserControl
    {
        protected BaseControl()
        {
            this.PreventFocusLoss();
        }
    }
}
