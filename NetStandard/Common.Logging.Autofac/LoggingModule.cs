using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Microsoft.Extensions.Logging;
using Module = Autofac.Module;

namespace Scar.Common.Logging.Autofac
{
    public sealed class LoggingModule : Module
    {
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            _ = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _ = registration ?? throw new ArgumentNullException(nameof(registration));
            registration.Preparing += OnComponentPreparing;
            base.AttachToComponentRegistration(componentRegistry, registration);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => GetLogger(typeof(LoggingModule))).As<ILogger>();
        }

        static ILogger GetLogger(Type t)
        {
            var typeName = GetTypePrintableName(t);
            return LoggerFactory.GetLogger(typeName);
        }

        static string GetTypePrintableName(Type type)
        {
            string typeName;
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == null)
                {
                    throw new InvalidOperationException("Type lacks generic type definition");
                }

                var generics = string.Join(",", type.GenericTypeArguments.Select(GetTypePrintableName));
                typeName = $"{type.Name.Remove(genericTypeDefinition.Name.IndexOf('`'))}<{generics}>";
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }

        static void OnComponentPreparing(object sender, PreparingEventArgs e)
        {
            _ = sender ?? throw new ArgumentNullException(nameof(sender));
            _ = e ?? throw new ArgumentNullException(nameof(e));
            var limitType = e.Component.Activator.LimitType;
            if (limitType.IsArray)
            {
                return;
            }

            var logger = GetLogger(limitType);
            e.Parameters = e.Parameters.Union(
                new[]
                {
                    new TypedParameter(typeof(ILogger), logger)
                });
        }
    }
}
