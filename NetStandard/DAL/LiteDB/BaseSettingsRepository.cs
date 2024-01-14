using System;
using Newtonsoft.Json;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.LiteDB;

public abstract class BaseSettingsRepository(string directoryPath, string fileName, bool shrink = true,
        bool isShared = false, bool isReadonly = false, bool requireUpgrade = true)
    : TrackedLiteDbRepository<ApplicationSettings, string>(directoryPath,
        fileName,
        shrink,
        isShared,
        isReadonly,
        requireUpgrade)
{
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

#pragma warning disable CS8603 // Possible null reference return.
    protected TValue TryGetValue<TValue>(string key, Func<TValue> defaultValueProvider)
    {
        _ = defaultValueProvider ?? throw new ArgumentNullException(nameof(defaultValueProvider));

        var entry = TryGetById(key);
        return entry == null ? defaultValueProvider() : JsonConvert.DeserializeObject<TValue>(entry.ValueJson);
    }

#pragma warning disable CS8601 // Possible null reference assignment.
    protected TValue TryGetValue<TValue>(string key, TValue defaultValue = default)
#pragma warning restore CS8601 // Possible null reference assignment.
    {
        var entry = TryGetById(key);
        return entry == null ? defaultValue : JsonConvert.DeserializeObject<TValue>(entry.ValueJson);
    }
#pragma warning restore CS8603 // Possible null reference return.
}
