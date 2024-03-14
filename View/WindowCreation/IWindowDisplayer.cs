using System;
using Scar.Common.View.Contracts;

namespace Scar.Common.View.WindowCreation;

public interface IWindowDisplayer
{
    Action<Action<TWindow>> DisplayWindow<TWindow>(Func<TWindow> createWindow)
        where TWindow : class, IDisplayable;
}