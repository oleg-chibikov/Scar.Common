using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;
using Scar.Common.DAL.Contracts;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.LiteDB
{
    public abstract class LiteDbRepository<T, TId> : FileBasedLiteDbRepository<TId>, IDisposableRepository<T, TId>, IChangeableRepository
        where T : IEntity<TId>
    {
        protected LiteDbRepository(string directoryPath, string? fileName = null, bool shrink = true, bool isShared = false, bool isReadonly = false, bool requireUpgrade = true) : base(
            directoryPath,
            fileName ?? typeof(T).Name,
            shrink,
            isShared,
            isReadonly,
            requireUpgrade)
        {
            Collection = Db.GetCollection<T>();
            if (!isReadonly)
            {
                Collection.EnsureIndex(x => x.Id, true);
            }
        }

        public event EventHandler? Changed;

        protected ILiteCollection<T> Collection { get; }

        public bool Check(TId id)
        {
            if (Equals(id, default))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return Collection.Exists(Query.EQ("_id", ToBson(id)));
        }

        public bool Delete(TId id)
        {
            if (Equals(id, default))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var deleted = Collection.Delete(ToBson(id));
            if (deleted)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return deleted;
        }

        public bool Delete(T entity)
        {
            if (Equals(entity, default))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (Equals(entity.Id, default))
            {
                throw new ArgumentException(nameof(entity.Id));
            }

            var deleted = Collection.Delete(ToBson(entity.Id));
            if (deleted)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return deleted;
        }

        public int Delete(IEnumerable<T> entities)
        {
            _ = entities ?? throw new ArgumentNullException(nameof(entities));

            // https://github.com/mbdavid/LiteDB/issues/318 - need to write _id instead of Id
            var deletedCount = Collection.DeleteMany(Query.In("_id", entities.Select(entity => ToBson(entity.Id))));
            if (deletedCount > 0)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return deletedCount;
        }

        public int Delete(IEnumerable<TId> ids)
        {
            _ = ids ?? throw new ArgumentNullException(nameof(ids));

            // https://github.com/mbdavid/LiteDB/issues/318 - need to write _id instead of Id
            var deletedCount = Collection.DeleteMany(Query.In("_id", ids.Select(id => ToBson(id))));
            if (deletedCount > 0)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return deletedCount;
        }

        public ICollection<T> Find(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
        {
            _ = predicate ?? throw new ArgumentNullException(nameof(predicate));
            return Collection.Find(predicate, pageNumber * pageSize, pageSize).ToArray();
        }

        public ICollection<T> GetAll()
        {
            return Collection.FindAll().ToArray();
        }

        public T GetById(TId id)
        {
            var entity = TryGetById(id);
            if (Equals(entity, default))
            {
                throw new InvalidOperationException($"No record for {id}");
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            return entity;
        }

        public ICollection<T> GetPage(int pageNumber, int pageSize, string? sortField, SortOrder sortOrder)
        {
            return Collection.Find(Query.All(sortField ?? "_id", sortOrder == SortOrder.Ascending ? Query.Ascending : Query.Descending), pageNumber * pageSize, pageSize).ToArray();
        }

        public TId Insert(T entity, bool skipCustomAction = false)
        {
            if (Equals(entity, default))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!skipCustomAction)
            {
                UpdateBeforeSave(entity, false);
            }

            GenerateIdIfNeeded(entity);

            var insertedId = Collection.Insert(entity);
            Changed?.Invoke(this, new EventArgs());

            return FromBson(insertedId);
        }

        public int Insert(IEnumerable<T> entities, bool skipCustomAction = false)
        {
            _ = entities ?? throw new ArgumentNullException(nameof(entities));
            var array = entities.ToArray();

            if (!skipCustomAction)
            {
                UpdateBeforeSave(array, false);
            }

            var countInserted = Collection.Insert(array);
            Changed?.Invoke(this, new EventArgs());
            return countInserted;
        }

        public T TryGetById(TId id)
        {
            if (Equals(id, default))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return Collection.FindById(ToBson(id));
        }

        public bool Update(T entity, bool skipCustomAction = false)
        {
            if (Equals(entity, default))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!skipCustomAction)
            {
                UpdateBeforeSave(entity, true);
            }

            var updated = Collection.Update(entity);
            if (updated)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return updated;
        }

        public int Update(IEnumerable<T> entities, bool skipCustomAction = false)
        {
            _ = entities ?? throw new ArgumentNullException(nameof(entities));
            var arr = entities.ToArray();

            if (!skipCustomAction)
            {
                UpdateBeforeSave(arr, true);
            }

            var countUpdated = Collection.Update(arr);
            if (countUpdated > 0)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return countUpdated;
        }

        public bool Upsert(T entity, bool skipCustomAction = false)
        {
            if (Equals(entity, default))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!skipCustomAction)
            {
                UpdateBeforeSave(entity, false);
            }

            GenerateIdIfNeeded(entity);

            var inserted = Collection.Upsert(entity);
            Changed?.Invoke(this, new EventArgs());
            return inserted;
        }

        public int Upsert(IEnumerable<T> entities, bool skipCustomAction = false)
        {
            _ = entities ?? throw new ArgumentNullException(nameof(entities));
            var arr = entities.ToArray();

            if (!skipCustomAction)
            {
                UpdateBeforeSave(arr, false);
            }

            var countInserted = Collection.Upsert(arr);
            Changed?.Invoke(this, new EventArgs());
            return countInserted;
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            _ = predicate ?? throw new ArgumentNullException(nameof(predicate));

            return Collection.Exists(predicate);
        }

        public int Clear() => Collection.DeleteAll();

        protected virtual void UpdateBeforeSave(T entity, bool isUpdate)
        {
        }

        protected virtual void UpdateBeforeSave(IEnumerable<T> entities, bool isUpdate)
        {
        }

        void GenerateIdIfNeeded(T entity)
        {
            if (!(entity is IMutableEntity<TId> mutableEntity) || !Equals(entity.Id, default))
            {
                return;
            }

            var generatedId = GenerateId();
            if (!Equals(generatedId, default))
            {
                mutableEntity.SetId(generatedId);
            }
        }
    }
}
