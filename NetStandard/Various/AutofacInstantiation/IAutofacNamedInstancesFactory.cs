using Autofac.Core;

namespace Scar.Common.AutofacInstantiation
{
    public interface IAutofacNamedInstancesFactory
    {
        T GetInstance<T>(params Parameter[] parameters)
            where T : class;
    }
}
