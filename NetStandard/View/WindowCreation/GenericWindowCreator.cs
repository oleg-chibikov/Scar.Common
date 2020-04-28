using System;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common
{
    public class GenericWindowCreator<TWindow> : IWindowCreator<TWindow>
        where TWindow : IDisplayable
    {
        readonly Func<TWindow> _windowFactory;

        public GenericWindowCreator(Func<TWindow> windowFactory)
        {
            _windowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Scar_Async", "Scar_Async_002:Non async method must not end with Async", Justification = "No need to make it Async")]
        public Task<TWindow> CreateWindowAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_windowFactory());
        }
    }
}
