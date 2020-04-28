using System;
using System.Net.Http;
using Autofac;

namespace Scar.Common.AutofacHttpClientProvision
{
    public static class ContainerBuilderHttpExtensions
    {
        public static void RegisterHttpClient<TServiceInstance>(this ContainerBuilder builder, string? baseAddress = null, Action<HttpClient>? setup = null)
        {
            builder.RegisterModule(
                new HttpClientModule<TServiceInstance>(
                    client =>
                    {
                        if (baseAddress != null)
                        {
                            client.BaseAddress = new Uri(baseAddress);
                        }

                        setup?.Invoke(client);
                    }));
        }
    }
}
