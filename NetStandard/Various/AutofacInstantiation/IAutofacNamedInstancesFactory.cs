using Autofac.Core;

namespace Scar.Common
{
    public interface IAutofacNamedInstancesFactory
    {
        T GetInstance<T>(params Parameter[] parameters)
            where T : class;
    }
}
