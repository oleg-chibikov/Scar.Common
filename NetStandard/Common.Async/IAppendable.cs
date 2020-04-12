namespace Scar.Common.Async
{
    public interface IAppendable<in T>
    {
        void Append(T t);
    }
}