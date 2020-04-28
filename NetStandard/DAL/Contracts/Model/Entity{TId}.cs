namespace Scar.Common.DAL.Contracts.Model
{
    public abstract class Entity<TId> : IEntity<TId>
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Initialized while parsing
        public TId Id { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public override string ToString()
        {
            return Id?.ToString() ?? string.Empty;
        }
    }
}
