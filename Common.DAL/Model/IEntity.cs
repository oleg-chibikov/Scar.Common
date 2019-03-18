namespace Scar.Common.DAL.Model
{
    public interface IEntity<TId>
    {
        TId Id { get; set; }
    }

    public interface IEntity : IEntity<object>
    {
    }
}