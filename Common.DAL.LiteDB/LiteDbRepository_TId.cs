using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using LiteDB;
using Scar.Common.DAL.Model;

namespace Scar.Common.DAL.LiteDB
{
    public abstract class LiteDbRepository<T, TId> : FileBasedLiteDbRepository<TId>, IRepository<T, TId>, IChangeableRepository
        where T : IEntity<TId>, new()
    {
        [NotNull]
        protected readonly LiteCollection<T> Collection;

        protected LiteDbRepository([NotNull] string directoryPath, [CanBeNull] string fileName = null, bool shrink = true)
            : base(directoryPath, fileName ?? typeof(T).Name, shrink)
        {
            Collection = Db.GetCollection<T>();
            Collection.EnsureIndex(x => x.Id, true);
        }

        public event EventHandler Changed;

        public bool Check([NotNull] TId id)
        {
            if (Equals(id, default(TId)))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return Collection.Exists(Query.EQ("_id", ToBson(id)));
        }

        public bool Delete([NotNull] TId id)
        {
            if (Equals(id, default(TId)))
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

        public bool Delete([NotNull] T entity)
        {
            if (Equals(entity, default(T)))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (Equals(entity.Id, default(TId)))
            {
                throw new ArgumentNullException(nameof(entity.Id));
            }

            var deleted = Collection.Delete(ToBson(entity.Id));
            if (deleted)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return deleted;
        }

        public int Delete([NotNull] IEnumerable<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            // https://github.com/mbdavid/LiteDB/issues/318 - need to write _id instead of Id
            var deletedCount = Collection.Delete(Query.In("_id", entities.Select(entity => ToBson(entity.Id))));
            if (deletedCount > 0)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return deletedCount;
        }

        public int Delete([NotNull] IEnumerable<TId> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            // https://github.com/mbdavid/LiteDB/issues/318 - need to write _id instead of Id
            var deletedCount = Collection.Delete(Query.In("_id", ids.Select(id => ToBson(id))));
            if (deletedCount > 0)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return deletedCount;
        }

        [NotNull]
        public ICollection<T> Get([NotNull] Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return Collection.Find(predicate, pageNumber * pageSize, pageSize).ToArray();
        }

        [NotNull]
        public ICollection<T> GetAll()
        {
            return Collection.FindAll().ToArray();
        }

        [NotNull]
        public T GetById([NotNull] TId id)
        {
            var entity = TryGetById(id);
            if (Equals(entity, default(T)))
            {
                throw new InvalidOperationException($"No record for {id}");
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            return entity;
        }

        [NotNull]
        public ICollection<T> GetPage(int pageNumber, int pageSize, [CanBeNull] string sortField, SortOrder sortOrder)
        {
            return Collection.Find(Query.All(sortField ?? "_id", sortOrder == SortOrder.Ascending ? Query.Ascending : Query.Descending), pageNumber * pageSize, pageSize).ToArray();
        }

        public TId Insert([NotNull] T entity, bool skipCustomAction = false)
        {
            if (Equals(entity, default(T)))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!skipCustomAction)
            {
                UpdateBeforeSave(entity);
            }

            var generatedId = GenerateId();

            if (!Equals(generatedId, default(TId)))
            {
                entity.Id = generatedId;
            }

            var insertedId = Collection.Insert(entity);
            Changed?.Invoke(this, new EventArgs());

            return FromBson(insertedId);
        }

        public int Insert([NotNull] IEnumerable<T> entities, bool skipCustomAction = false)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var arr = entities.ToArray();

            if (!skipCustomAction)
            {
                UpdateBeforeSave(arr);
            }

            var countInserted = Collection.Insert(arr);
            Changed?.Invoke(this, new EventArgs());
            return countInserted;
        }

        [CanBeNull]
        public T TryGetById([NotNull] TId id)
        {
            if (Equals(id, default(TId)))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return Collection.FindById(ToBson(id));
        }

        public bool Update([NotNull] T entity, bool skipCustomAction = false)
        {
            if (Equals(entity, default(T)))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!skipCustomAction)
            {
                UpdateBeforeSave(entity);
            }

            var updated = Collection.Update(entity);
            if (updated)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return updated;
        }

        public int Update([NotNull] IEnumerable<T> entities, bool skipCustomAction = false)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var arr = entities.ToArray();

            if (!skipCustomAction)
            {
                UpdateBeforeSave(arr);
            }

            var countUpdated = Collection.Update(arr);
            if (countUpdated > 0)
            {
                Changed?.Invoke(this, new EventArgs());
            }

            return countUpdated;
        }

        public bool Upsert([NotNull] T entity, bool skipCustomAction = false)
        {
            if (Equals(entity, default(T)))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!skipCustomAction)
            {
                UpdateBeforeSave(entity);
            }

            var inserted = Collection.Upsert(entity);
            Changed?.Invoke(this, new EventArgs());
            return inserted;
        }

        public int Upsert([NotNull] IEnumerable<T> entities, bool skipCustomAction = false)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var arr = entities.ToArray();

            if (!skipCustomAction)
            {
                UpdateBeforeSave(arr);
            }

            var countInserted = Collection.Upsert(arr);
            Changed?.Invoke(this, new EventArgs());
            return countInserted;
        }

        protected virtual void UpdateBeforeSave([NotNull] T entity)
        {
        }

        protected virtual void UpdateBeforeSave([NotNull] IEnumerable<T> entities)
        {
        }
    }
}