using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Scar.Common.DAL.Model;

namespace Scar.Common.DAL
{
    public interface IRepository<T, TId>
        where T : IEntity<TId>
    {
        bool Check([NotNull] TId id);

        bool Delete([NotNull] T entity);

        bool Delete([NotNull] TId id);

        int Delete([NotNull] IEnumerable<T> entities);

        int Delete([NotNull] IEnumerable<TId> ids);

        [NotNull]
        ICollection<T> Get([NotNull] Expression<Func<T, bool>> predicate, int pageNumber = 0, int pageSize = int.MaxValue);

        [NotNull]
        ICollection<T> GetAll();

        [NotNull]
        T GetById([NotNull] TId id);

        [NotNull]
        ICollection<T> GetPage(int pageNumber, int pageSize, [CanBeNull] string sortField = null, SortOrder sortOrder = SortOrder.Ascending);

        [NotNull]
        TId Insert([NotNull] T entity, bool skipCustomAction = false);

        int Insert([NotNull] IEnumerable<T> entities, bool skipCustomAction = false);

        [CanBeNull]
        T TryGetById([NotNull] TId id);

        bool Update([NotNull] T entity, bool skipCustomAction = false);

        int Update([NotNull] IEnumerable<T> entities, bool skipCustomAction = false);

        bool Upsert([NotNull] T entity, bool skipCustomAction = false);

        int Upsert([NotNull] IEnumerable<T> entities, bool skipCustomAction = false);
    }

    public interface IRepository<T> : IRepository<T, object>
        where T : IEntity
    {
    }
}