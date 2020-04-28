using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Scar.Common.WPF.Controls
{
    /// <summary>
    /// Opens <see cref="Hyperlink.NavigateUri" /> in a default system browser.
    /// </summary>
    public sealed class ExternalBrowserHyperlink : Hyperlink
    {
        public ExternalBrowserHyperlink()
        {
            RequestNavigate += OnRequestNavigate;
        }

        static void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            using var process = new Process { StartInfo = new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true } };
            process.Start();
            e.Handled = true;
        }
    }
}
