using System;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;
using Scar.Common.DAL.Model;

namespace Scar.Common.DAL.LiteDB
{
    public abstract class MultiTypeSettingsLiteDbRepository : FileBasedLiteDbRepository<string>, IMultiTypeSettingsRepository, IObjectCrudRepository<string>
    {
        private readonly ConcurrentDictionary<Type, string> _collectionNames = new ConcurrentDictionary<Type, string>();

        protected MultiTypeSettingsLiteDbRepository([NotNull] string directoryPath, [NotNull] string fileName, bool shrink = true)
            : base(directoryPath, fileName, shrink)
        {
        }

        public void RemoveUpdateOrInsert<TValue>([NotNull] string key, [NotNull] TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (Equals(value, default(TValue)))
            {
                Delete(key, typeof(TValue));
            }
            else
            {
                var settings = CreateSettings(key, value);
                if (!Update(settings, typeof(TValue)))
                {
                    Insert(settings, typeof(TValue));
                }
            }
        }

        [CanBeNull]
        public TValue TryGetValue<TValue>([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var settings = Db.GetCollection<MutliTypeSettings<TValue>>(GetCollectionName(typeof(TValue))).FindById(ToBson(key));
            return settings == null ? default(TValue) : settings.Value;
        }

        public bool DeleteObject([NotNull] object entity, [NotNull] Type objectType)
        {
            return Delete((IMutliTypeSettings)entity, objectType);
        }

        public string InsertObject([NotNull] object entity, [NotNull] Type objectType)
        {
            return Insert((IMutliTypeSettings)entity, objectType);
        }

        public bool UpdateObject([NotNull] object entity, [NotNull] Type objectType)
        {
            return Update((IMutliTypeSettings)entity, objectType);
        }

        [CanBeNull]
        public object TryGetObjectById([NotNull] string id, [NotNull] Type objectType)
        {
            return Db.GetCollection<object>(GetCollectionName(objectType)).FindById(ToBson(id));
        }

        [NotNull]
        protected virtual MutliTypeSettings<TValue> CreateSettings<TValue>(string key, TValue value)
        {
            return new MutliTypeSettings<TValue>(key, value);
        }

        private bool Delete([NotNull] IMutliTypeSettings entity, [NotNull] Type objectType)
        {
            return Db.GetCollection(GetCollectionName(objectType)).Delete(ToBson(entity.Id));
        }

        private void Delete([NotNull] string key, [NotNull] Type objectType)
        {
            Db.GetCollection(GetCollectionName(objectType)).Delete(ToBson(key));
        }

        private string Insert([NotNull] IMutliTypeSettings entity, [NotNull] Type objectType)
        {
            UpdateBeforeSave(entity);
            var generatedId = GenerateId();
            if (!Equals(generatedId, default(string)))
            {
                entity.Id = generatedId;
            }

            var inserted = Db.GetCollection(GetCollectionName(objectType)).Insert(entity);
            return FromBson(inserted);
        }

        [NotNull]
        private string GetCollectionName([NotNull] Type type)
        {
            return _collectionNames.GetOrAdd(type, x => GetFriendlyName(type));
        }

        private bool Update([NotNull] IMutliTypeSettings entity, [NotNull] Type objectType)
        {
            UpdateBeforeSave(entity);
            return Db.GetCollection(GetCollectionName(objectType)).Update(entity);
        }

        protected virtual void UpdateBeforeSave([NotNull] IMutliTypeSettings entity)
        {
        }

        [NotNull]
        private static string GetFriendlyName([NotNull] Type type)
        {
            if (type.IsGenericType)
            {
                return type.Name.Split('`')[0] + "-" + string.Join("-", type.GetGenericArguments().Select(GetFriendlyName));
            }

            return type.FullName.Replace(".", "-");
        }
    }
}