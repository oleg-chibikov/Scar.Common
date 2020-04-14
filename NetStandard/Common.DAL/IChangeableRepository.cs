using System;

namespace Scar.Common.DAL
{
    public interface IChangeableRepository
    {
        event EventHandler Changed;
    }
}
