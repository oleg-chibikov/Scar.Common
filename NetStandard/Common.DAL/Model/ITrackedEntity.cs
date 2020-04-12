using System;

namespace Scar.Common.DAL.Model
{
    public interface ITrackedEntity
    {
        DateTime ModifiedDate { get; set; }
        DateTime CreatedDate { get; set; }
    }
}