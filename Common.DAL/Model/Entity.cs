namespace Scar.Common.DAL.Model
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public abstract class Entity<TId> : IEntity<TId>
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    {
        public TId Id { get; set; }

        public override string ToString()
        {
            return Id?.ToString() ?? string.Empty;
        }
    }

    public abstract class Entity : Entity<object>, IEntity
    {
    }
}