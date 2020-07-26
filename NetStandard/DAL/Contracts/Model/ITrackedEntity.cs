using System;

namespace Scar.Common.DAL.Contracts.Model
{
    public interface ITrackedEntity
    {
        DateTimeOffset ModifiedDate { get; set; }

        DateTimeOffset CreatedDate { get; set; }
    }
}
