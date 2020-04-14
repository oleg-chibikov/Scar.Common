using System;

namespace Scar.Common.Events
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T parameter)
        {
            Parameter = parameter;
        }

        public T Parameter { get; }
    }
}
