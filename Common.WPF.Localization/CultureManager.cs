using JetBrains.Annotations;
using Scar.Common.Localization;
using System.Globalization;

namespace Scar.Common.WPF.Localization
{
    public class CultureManager : ICultureManager
    {
        public void ChangeCulture([NotNull] CultureInfo cultureInfo)
        {
            CultureUtilities.ChangeCulture(cultureInfo);
        }
    }
}
