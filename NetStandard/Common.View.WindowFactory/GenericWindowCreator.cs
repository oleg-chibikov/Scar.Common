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

        public Task<TWindow> CreateWindowAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_windowFactory());
        }
    }
}
