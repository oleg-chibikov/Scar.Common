using System;

namespace Scar.Common.Events;

public class EventArgs<T>(T parameter) : EventArgs
{
    public T Parameter { get; } = parameter;
}
