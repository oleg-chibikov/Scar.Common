using System.Windows.Controls;

namespace Scar.Common.WPF.Core;

public class BaseControl : UserControl
{
    protected BaseControl()
    {
        this.PreventFocusLoss();
    }
}
