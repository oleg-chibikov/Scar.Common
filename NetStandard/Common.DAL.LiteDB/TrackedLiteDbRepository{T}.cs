using LiteDB;
using Scar.Common.DAL.Model;

namespace Scar.Common.DAL.LiteDB
{
    public abstract class TrackedLiteDbRepository<T> : TrackedLiteDbRepository<T, object>
        where T : Entity, ITrackedEntity, new()
    {
        protected TrackedLiteDbRepository(string directoryPath, string? fileName = null, bool shrink = true) : base(directoryPath, fileName, shrink)
        {
        }

        protected override bool IsBson => true;

        protected override object GenerateId()
        {
            return ObjectId.NewObjectId();
        }
    }
}
