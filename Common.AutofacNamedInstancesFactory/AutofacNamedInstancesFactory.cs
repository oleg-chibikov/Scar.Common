using System;
using Autofac;
using Autofac.Core;

namespace Scar.Common.AutofacNamedInstancesFactory
{
    public sealed class AutofacNamedInstancesFactory : IAutofacNamedInstancesFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacNamedInstancesFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
        }

        public T GetInstance<T>(params Parameter[] parameters)
        {
            return _lifetimeScope.ResolveNamed<T>(typeof(T).FullName, parameters);
        }
    }
}