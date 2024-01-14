using System;
using System.Linq;
using System.Net.Http;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

namespace Scar.Common.AutofacHttpClientProvision;

public class HttpClientModule<TService>(Action<HttpClient>? clientConfigurator = null) : Module
{
    protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
    {
        _ = registration ?? throw new ArgumentNullException(nameof(registration));

        base.AttachToComponentRegistration(componentRegistry, registration);

        if (registration.Activator.LimitType == typeof(TService))
        {
            registration.PipelineBuilding += (_, pipeline) =>
            {
                pipeline.Use(
                    PipelinePhase.ParameterSelection,
                    (context, next) =>
                    {
                        context.ChangeParameters(
                            context.Parameters.Union(
                                new[]
                                {
                                    new ResolvedParameter(
                                        (p, _) => p.ParameterType == typeof(HttpClient),
                                        (_, i) =>
                                        {
                                            var client = i.Resolve<IHttpClientFactory>().CreateClient();
                                            clientConfigurator?.Invoke(client);
                                            return client;
                                        })
                                }));
                        next(context);
                    });
            };
        }
    }
}
