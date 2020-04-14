using System;

namespace Scar.Common.NamedPipes
{
    public interface INamedPipesServer<T> : IDisposable
    {
        event EventHandler<MessageEventArgs<T>> MessageReceived;
    }
}
