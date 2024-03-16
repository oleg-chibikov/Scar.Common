using System;

namespace Scar.Common.DAL.Contracts.Model;

public abstract class TrackedEntity<TId> : Entity<TId>, ITrackedEntity
{
    public DateTimeOffset ModifiedDate { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}
