using System;

namespace Scar.Common.DAL.Model
{
    public abstract class TrackedEntity<TId> : Entity<TId>, ITrackedEntity
    {
        public DateTime ModifiedDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
