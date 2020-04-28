using System;

namespace Scar.Common.DAL.Contracts
{
    public interface IChangeableRepository
    {
        event EventHandler Changed;
    }
}
