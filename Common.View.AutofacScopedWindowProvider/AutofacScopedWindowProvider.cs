using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Scar.Common.View.Contracts;
using Scar.Common.View.WindowFactory;

namespace Scar.Common.View
{
    public class AutofacScopedWindowProvider : IScopedWindowProvider
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacScopedWindowProvider(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
        }

        public async Task<TWindow> GetScopedWindowAsync<TWindow, TParam>(TParam param, CancellationToken cancellationToken)
            where TWindow : IDisplayable
        {
            var nestedLifeTimeScope = _lifetimeScope.BeginLifetimeScope();
            TWindow window;
            if (!Equals(param, null))
            {
                var windowCreator = nestedLifeTimeScope.Resolve<IWindowCreator<TWindow, TParam>>();
                window = await windowCreator.CreateWindowAsync(param, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var windowCreator = nestedLifeTimeScope.Resolve<IWindowCreator<TWindow>>();
                window = await windowCreator.CreateWindowAsync(cancellationToken).ConfigureAwait(false);
            }

            window.AssociateDisposable(nestedLifeTimeScope);
            return window;
        }
    }
}