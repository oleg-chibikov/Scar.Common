using System;
using System.Threading.Tasks;

namespace Scar.Common.WebApi
{
    public interface IApiHoster
    {
        Uri BaseUrl { get; }

        Task RegisterWebApiHostAsync();
    }
}
