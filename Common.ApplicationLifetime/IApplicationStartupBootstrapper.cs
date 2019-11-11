using System;
using System.Threading;
using Autofac;
using Common.Logging;
using Easy.MessageHub;
using Scar.Common.Localization;
using Scar.Common.Messages;

namespace Scar.Common.ApplicationLifetime
{
    public interface IApplicationStartupBootstrapper
    {
        string AppGuid { get; }
        Action<Message> ShowMessage { get; }
        ILifetimeScope Container { get; }
        ILog Logger { get; }
        IMessageHub Messenger { get; }
        ICultureManager CultureManager { get; }
        SynchronizationContext? SynchronizationContext { get; }

        void HandleException(Exception e);

        void OnExit();

        void OnStart();
    }
}