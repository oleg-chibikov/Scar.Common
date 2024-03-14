using LiteDB;
using Scar.Common.DAL.Contracts;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.LiteDB;

public abstract class LiteDbRepository<T>(string directoryPath, string? fileName = null, bool shrink = true,
        bool isShared = false, bool isReadonly = false, bool requireUpgrade = true)
    : LiteDbRepository<T, object>(directoryPath,
        fileName,
        shrink,
        isShared,
        isReadonly,
        requireUpgrade), IRepository<T>
    where T : IEntity
{
    protected override bool IsBson => true;

    protected override object GenerateId()
    {
        return ObjectId.NewObjectId();
    }
}
