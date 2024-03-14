namespace Scar.Common.DAL.Contracts.Model;

public interface IMutableEntity<TId> : IEntity<TId>
{
    void SetId(TId id);
}