using LiteDB;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.LiteDB;

public abstract class TrackedLiteDbRepository<T>(string directoryPath, string? fileName = null, bool shrink = true,
        bool isShared = false, bool isReadonly = false, bool requireUpgrade = true)
    : TrackedLiteDbRepository<T, object>(directoryPath,
        fileName,
        shrink,
        isShared,
        isReadonly,
        requireUpgrade)
    where T : IEntity, ITrackedEntity
{
    protected override bool IsBson => true;

    protected override object GenerateId()
    {
        return ObjectId.NewObjectId();
    }
}
