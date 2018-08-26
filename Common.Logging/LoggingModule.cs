using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Common.Logging;
using JetBrains.Annotations;
using Module = Autofac.Module;

namespace Scar.Common.Logging
{
    public sealed class LoggingModule : Module
    {
        protected override void AttachToComponentRegistration([NotNull] IComponentRegistry componentRegistry, [NotNull] IComponentRegistration registration)
        {
            if (componentRegistry == null)
            {
                throw new ArgumentNullException(nameof(componentRegistry));
            }

            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.Preparing += OnComponentPreparing;
            registration.Activated += (sender, e) => InjectLoggerProperties(e.Instance);
        }

        [NotNull]
        private static ILog GetLogger([NotNull] Type t)
        {
            var typeName = GetTypePrintableName(t);
            return LogManager.GetLogger(typeName);
        }

        [NotNull]
        private static string GetTypePrintableName([NotNull] Type type)
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

        private static void InjectLoggerProperties([NotNull] object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var instanceType = instance.GetType();

            // Get all the injectable properties to set.
            // If you wanted to ensure the properties were only UNSET properties,
            // here's where you'd do it.
            var properties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(ILog) && p.CanWrite && p.GetIndexParameters().Length == 0);

            // Set the properties located.
            foreach (var propToSet in properties)
            {
                propToSet.SetValue(instance, GetLogger(instanceType), null);
            }
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => GetLogger(typeof(LoggingModule))).As<ILog>();
        }

        private static void OnComponentPreparing([NotNull] object sender, [NotNull] PreparingEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            var limitType = e.Component.Activator.LimitType;
            if (limitType.IsArray)
            {
                return;
            }

            var logger = GetLogger(limitType);
            e.Parameters = e.Parameters.Union(
                new[]
                {
                    new TypedParameter(typeof(ILog), logger)
                });
        }
    }
}