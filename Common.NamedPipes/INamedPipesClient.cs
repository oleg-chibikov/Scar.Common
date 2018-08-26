using JetBrains.Annotations;

namespace Scar.Common.NamedPipes
{
    public interface INamedPipesClient<in T>
    {
        void SendMessage([NotNull] T message, int timeout = 100);
    }
}