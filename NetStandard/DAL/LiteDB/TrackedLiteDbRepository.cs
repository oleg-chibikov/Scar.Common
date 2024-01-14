using System;
using System.Collections.Generic;
using System.Linq;
using Scar.Common.DAL.Contracts;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.LiteDB;

public abstract class TrackedLiteDbRepository<T, TId> : LiteDbRepository<T, TId>, ITrackedRepository
    where T : IEntity<TId>, ITrackedEntity
{
    protected TrackedLiteDbRepository(string directoryPath, string? fileName = null, bool shrink = true, bool isShared = false, bool isReadonly = false, bool requireUpgrade = true) : base(
        directoryPath,
        fileName,
        shrink,
        isShared,
        isReadonly,
        requireUpgrade)
    {
        if (!isReadonly)
        {
            Collection.EnsureIndex(x => x.ModifiedDate);
            Collection.EnsureIndex(x => x.CreatedDate);
        }
    }

    public ICollection<ITrackedEntity> GetModifiedAfter(DateTimeOffset startExclusive)
    {
        return Collection.Find(x => x.ModifiedDate > startExclusive).Cast<ITrackedEntity>().ToArray();
    }

    public ICollection<ITrackedEntity> GetModifiedBefore(DateTimeOffset endExclusive)
    {
        return Collection.Find(x => x.ModifiedDate < endExclusive).Cast<ITrackedEntity>().ToArray();
    }

    public ICollection<ITrackedEntity> GetModifiedBetween(DateTimeOffset startInclusive, DateTimeOffset endExclusive)
    {
        return Collection.Find(x => (x.ModifiedDate >= startInclusive) && (x.ModifiedDate < endExclusive)).Cast<ITrackedEntity>().ToArray();
    }

    public ICollection<ITrackedEntity> GetCreatedAfter(DateTimeOffset startExclusive)
    {
        return Collection.Find(x => x.CreatedDate > startExclusive).Cast<ITrackedEntity>().ToArray();
    }

    public ICollection<ITrackedEntity> GetCreatedBefore(DateTimeOffset endExclusive)
    {
        return Collection.Find(x => x.CreatedDate < endExclusive).Cast<ITrackedEntity>().ToArray();
    }

    public ICollection<ITrackedEntity> GetCreatedBetween(DateTimeOffset startInclusive, DateTimeOffset endExclusive)
    {
        return Collection.Find(x => (x.CreatedDate >= startInclusive) && (x.CreatedDate < endExclusive)).Cast<ITrackedEntity>().ToArray();
    }

    protected override void UpdateBeforeSave(IEnumerable<T> entities, bool isUpdate)
    {
        _ = entities ?? throw new ArgumentNullException(nameof(entities));

        foreach (var entity in entities)
        {
            UpdateBeforeSave(entity, isUpdate);
        }
    }

    protected override void UpdateBeforeSave(T entity, bool isUpdate)
    {
        var now = DateTimeOffset.Now;
        if (!isUpdate && entity.CreatedDate == default)
        {
            entity.CreatedDate = now;
        }

        entity.ModifiedDate = now;
    }
}