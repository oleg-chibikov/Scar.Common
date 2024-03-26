using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.Contracts;

public interface IRepository<T, TId>
    where T : IEntity<TId>
{
    bool Check(TId id);

    bool Delete(T entity);

    bool Delete(TId id);

    int Delete(IEnumerable<T> entities);

    int Delete(IEnumerable<TId> ids);

    IReadOnlyCollection<T> GetAll();

    int Count();

    IReadOnlyCollection<T> Find(Expression<Func<T, bool>> predicate, int pageNumber = 0, int pageSize = int.MaxValue);

    IReadOnlyCollection<T> GetPage(int pageNumber, int pageSize, string? sortField = null, SortOrder sortOrder = SortOrder.Ascending);

    public IEnumerable<T> EnumerateAll();

    IEnumerable<T> Enumerate(Expression<Func<T, bool>> predicate, int pageNumber = 0, int pageSize = int.MaxValue);

    IEnumerable<T> EnumeratePage(int pageNumber, int pageSize, string? sortField = null, SortOrder sortOrder = SortOrder.Ascending);

    T GetById(TId id);

    TId Insert(T entity, bool skipCustomAction = false);

    int Insert(IEnumerable<T> entities, bool skipCustomAction = false);

    T? TryGetById(TId id);

    bool Update(T entity, bool skipCustomAction = false);

    int Update(IEnumerable<T> entities, bool skipCustomAction = false);

    bool Upsert(T entity, bool skipCustomAction = false);

    int Upsert(IEnumerable<T> entities, bool skipCustomAction = false);

    public bool Exists(Expression<Func<T, bool>> predicate);

    public int Clear();
}

public interface IRepository<T> : IRepository<T, object>
    where T : IEntity<object>;
