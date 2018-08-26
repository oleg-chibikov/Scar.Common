using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Scar.Common.WPF.View.Contracts;

namespace Scar.Common.WPF.View
{
    [UsedImplicitly]
    public class GenericWindowCreator<TWindow> : IWindowCreator<TWindow>
        where TWindow : IWindow
    {
        [NotNull]
        private readonly Func<TWindow> _windowFactory;

        public GenericWindowCreator([NotNull] Func<TWindow> windowFactory)
        {
            _windowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));
        }

        public Task<TWindow> CreateWindowAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_windowFactory());
        }
    }
}