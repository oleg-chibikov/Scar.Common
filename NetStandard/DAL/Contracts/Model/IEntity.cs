namespace Scar.Common.DAL.Contracts.Model
{
    public interface IEntity<TId>
    {
        TId Id { get; set; }
    }

    public interface IEntity : IEntity<object>
    {
    }
}
