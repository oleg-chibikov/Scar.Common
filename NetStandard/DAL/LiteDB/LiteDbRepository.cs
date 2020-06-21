using LiteDB;
using Scar.Common.DAL.Contracts;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.LiteDB
{
    public abstract class LiteDbRepository<T> : LiteDbRepository<T, object>, IRepository<T>
        where T : IEntity
    {
        protected LiteDbRepository(string directoryPath, string? fileName = null, bool shrink = true, bool isShared = false, bool isReadonly = false, bool requireUpgrade = true) : base(
            directoryPath,
            fileName,
            shrink,
            isShared,
            isReadonly,
            requireUpgrade)
        {
        }

        protected override bool IsBson => true;

        protected override object GenerateId()
        {
            return ObjectId.NewObjectId();
        }
    }
}
