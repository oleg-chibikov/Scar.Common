using System;

namespace Scar.Common.DAL.Contracts.Model
{
    public interface ITrackedEntity
    {
        DateTime ModifiedDate { get; set; }

        DateTime CreatedDate { get; set; }
    }
}
