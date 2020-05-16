namespace Scar.Common.DAL.Contracts.Model
{
    public interface IEntity<out TId>
    {
        TId Id { get; }
    }
}
