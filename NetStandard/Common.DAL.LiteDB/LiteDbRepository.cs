using LiteDB;
using Scar.Common.DAL.Model;

namespace Scar.Common.DAL.LiteDB
{
    public abstract class LiteDbRepository<T> : LiteDbRepository<T, object>, IRepository<T>
        where T : Entity, new()
    {
        protected LiteDbRepository(string directoryPath, string? fileName = null, bool shrink = true)
            : base(directoryPath, fileName, shrink)
        {
        }

        protected override bool IsBson => true;

        protected override object GenerateId()
        {
            return ObjectId.NewObjectId();
        }
    }
}