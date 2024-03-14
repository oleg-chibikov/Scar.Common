using System.Globalization;

namespace Scar.Common.Localization;

public sealed class ConsoleCultureManager : ICultureManager
{
    public void ChangeCulture(CultureInfo cultureInfo)
    {
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
    }
}
