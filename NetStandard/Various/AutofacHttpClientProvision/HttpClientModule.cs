using System;
using System.Linq;
using System.Net.Http;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Scar.Common.AutofacHttpClientProvision
{
    public class HttpClientModule<TService> : Module
    {
        readonly Action<HttpClient>? _clientConfigurator;

        public HttpClientModule(Action<HttpClient>? clientConfigurator = null)
        {
            _clientConfigurator = clientConfigurator;
        }

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            _ = registration ?? throw new ArgumentNullException(nameof(registration));

            base.AttachToComponentRegistration(componentRegistry, registration);

            if (registration.Activator.LimitType == typeof(TService))
            {
                registration.Preparing += (sender, e) =>
                {
                    e.Parameters = e.Parameters.Union(
                        new[]
                        {
                            new ResolvedParameter(
                                (p, i) => p.ParameterType == typeof(HttpClient),
                                (p, i) =>
                                {
                                    var client = i.Resolve<IHttpClientFactory>().CreateClient();
                                    _clientConfigurator?.Invoke(client);
                                    return client;
                                })
                        });
                };
            }
        }
    }
}
