using System;

namespace Scar.Common.DAL.Contracts.Model
{
    public abstract class TrackedEntity : Entity, ITrackedEntity
    {
        public DateTimeOffset ModifiedDate { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
    }
}
