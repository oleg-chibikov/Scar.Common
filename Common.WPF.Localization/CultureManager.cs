using System.Globalization;
using Scar.Common.Localization;

namespace Scar.Common.WPF.Localization
{
    public class CultureManager : ICultureManager
    {
        public void ChangeCulture(CultureInfo cultureInfo)
        {
            CultureUtilities.ChangeCulture(cultureInfo);
        }
    }
}
