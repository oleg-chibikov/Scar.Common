using System;
using System.Collections.Generic;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.Contracts;

public interface ITrackedRepository
{
    ICollection<ITrackedEntity> GetModifiedAfter(DateTimeOffset startExclusive);

    ICollection<ITrackedEntity> GetModifiedBefore(DateTimeOffset endExclusive);

    ICollection<ITrackedEntity> GetModifiedBetween(DateTimeOffset startInclusive, DateTimeOffset endExclusive);

    ICollection<ITrackedEntity> GetCreatedAfter(DateTimeOffset startExclusive);

    ICollection<ITrackedEntity> GetCreatedBefore(DateTimeOffset endExclusive);

    ICollection<ITrackedEntity> GetCreatedBetween(DateTimeOffset startInclusive, DateTimeOffset endExclusive);
}
