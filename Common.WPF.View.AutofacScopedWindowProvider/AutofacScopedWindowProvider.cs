using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Scar.Common.WPF.View.Contracts;

namespace Scar.Common.WPF.View
{
    [UsedImplicitly]
    public class AutofacScopedWindowProvider : IScopedWindowProvider
    {
        [NotNull]
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacScopedWindowProvider([NotNull] ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
        }

        public async Task<TWindow> GetScopedWindowAsync<TWindow, TParam>(TParam param, CancellationToken cancellationToken)
            where TWindow : IWindow
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