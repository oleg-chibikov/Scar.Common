using Autofac.Core;

namespace Scar.Common.AutofacNamedInstancesFactory
{
    public interface IAutofacNamedInstancesFactory
    {
        T GetInstance<T>(params Parameter[] parameters);
    }
}
