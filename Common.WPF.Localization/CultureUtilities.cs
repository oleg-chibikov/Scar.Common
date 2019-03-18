using System;
using System.Globalization;
using JetBrains.Annotations;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace Scar.Common.WPF.Localization
{
    public static class CultureUtilities
    {
        [NotNull]
        private static readonly object Locker = new object();

        public static void ChangeCulture([NotNull] string uiLanguage)
        {
            if (uiLanguage == null)
            {
                throw new ArgumentNullException(nameof(uiLanguage));
            }

            var cultureInfo = CultureInfo.GetCultureInfo(uiLanguage);
            ChangeCulture(cultureInfo);
        }

        public static void ChangeCulture([NotNull] CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                throw new ArgumentNullException(nameof(cultureInfo));
            }

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

        [CanBeNull]
        public static T GetLocalizedValue<T, TResources>([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var type = typeof(TResources);
            return LocExtension.GetLocalizedValue<T>($"{type.Namespace}:{type.Name}:" + key);
        }
    }
}