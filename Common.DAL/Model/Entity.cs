namespace Scar.Common.DAL.Model
{
    public abstract class Entity<TId> : IEntity<TId>
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