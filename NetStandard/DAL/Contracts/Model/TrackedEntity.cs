using System;

namespace Scar.Common.DAL.Contracts.Model
{
    public abstract class TrackedEntity : Entity, ITrackedEntity
    {
        public DateTime ModifiedDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
