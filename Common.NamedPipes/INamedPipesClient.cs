namespace Scar.Common.NamedPipes
{
    public interface INamedPipesClient<in T>
    {
        void SendMessage(T message, int timeout = 100);
    }
}
