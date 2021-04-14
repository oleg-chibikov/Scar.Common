using System;
using Autofac;
using Autofac.Core;

namespace Scar.Common.AutofacInstantiation
{
    public sealed class AutofacNamedInstancesFactory : IAutofacNamedInstancesFactory
    {
        readonly ILifetimeScope _lifetimeScope;

        public AutofacNamedInstancesFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
        }

        public T GetInstance<T>(params Parameter[] parameters)
            where T : class
        {
            return _lifetimeScope.ResolveNamed<T>(typeof(T).FullName ?? throw new InvalidOperationException("Name of type is null"), parameters);
        }
    }
}
