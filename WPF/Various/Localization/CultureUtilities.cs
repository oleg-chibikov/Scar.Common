using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace Scar.Common.WPF.Localization
{
    public static class CultureUtilities
    {
        static readonly object Locker = new ();

        public static void ChangeCulture(string uiLanguage)
        {
            _ = uiLanguage ?? throw new ArgumentNullException(nameof(uiLanguage));
            var cultureInfo = CultureInfo.GetCultureInfo(uiLanguage);
            ChangeCulture(cultureInfo);
        }

        public static void ChangeCulture(CultureInfo cultureInfo)
        {
            _ = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));

            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            if (Equals(LocalizeDictionary.Instance.Culture, cultureInfo))
            {
                return;
            }

            lock (Locker)
            {
                if (Equals(LocalizeDictionary.Instance.Culture, cultureInfo))
                {
                    return;
                }

                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
                LocalizeDictionary.Instance.Culture = cultureInfo;
            }
        }

        [return: MaybeNull]
        public static T GetLocalizedValue<T, TResources>(string key)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            var type = typeof(TResources);
            var obj = LocalizeDictionary.Instance.GetLocalizedObject($"{type.Assembly.GetName().Name}:{type.Name}:" + key, null, LocalizeDictionary.Instance.Culture);
            return obj == null ? default! : (T)obj;
        }
    }
}
