using System;
using JetBrains.Annotations;

namespace Scar.Common.Events
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs([CanBeNull] T parameter)
        {
            Parameter = parameter;
        }

        [CanBeNull]
        public T Parameter { get; }
    }
}