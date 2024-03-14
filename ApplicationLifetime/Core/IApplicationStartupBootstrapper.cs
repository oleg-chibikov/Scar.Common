using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Easy.MessageHub;
using Scar.Common.Localization;
using Scar.Common.Messages;

namespace Scar.Common.ApplicationLifetime.Core;

public interface IApplicationStartupBootstrapper
{
    string AppGuid { get; }

    Action<Message> ShowMessage { get; }

    ILifetimeScope Container { get; }

    IMessageHub Messenger { get; }

    ICultureManager CultureManager { get; }

    SynchronizationContext? SynchronizationContext { get; }

    void HandleException(Exception e);

    Task OnExitAsync();

    void BeforeStart();

    Task OnStartAsync();
}
