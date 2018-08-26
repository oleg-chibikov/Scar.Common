using JetBrains.Annotations;

namespace Scar.Common.DAL.Model
{
    public interface IEntity<TId>
    {
        [CanBeNull]
        TId Id { get; set; }
    }

    public interface IEntity : IEntity<object>
    {
    }
}