using System;
using Newtonsoft.Json;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.LiteDB
{
    public abstract class BaseSettingsRepository : TrackedLiteDbRepository<ApplicationSettings, string>
    {
        protected BaseSettingsRepository(string directoryPath, string fileName, bool shrink = true, bool isShared = false, bool isReadonly = false, bool requireUpgrade = true) : base(
            directoryPath,
            fileName,
            shrink,
            isShared,
            isReadonly,
            requireUpgrade)
        {
        }

        protected void RemoveUpdateOrInsert<TValue>(string key, TValue value)
        {
            if (Equals(value, default))
            {
                Delete(key);
                return;
            }

            var settings = new ApplicationSettings { Id = key, ValueJson = JsonConvert.SerializeObject(value) };
            if (!Update(settings))
            {
                Insert(settings);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Analyzer error - it is validated")]
        protected TValue TryGetValue<TValue>(string key, Func<TValue> defaultValueProvider)
        {
            _ = defaultValueProvider ?? throw new ArgumentNullException(nameof(defaultValueProvider));

            var entry = TryGetById(key);
            return entry == null ? defaultValueProvider() : JsonConvert.DeserializeObject<TValue>(entry.ValueJson);
        }

        protected TValue TryGetValue<TValue>(string key, TValue defaultValue = default)
        {
            var entry = TryGetById(key);
            return entry == null ? defaultValue : JsonConvert.DeserializeObject<TValue>(entry.ValueJson);
        }
    }
}
