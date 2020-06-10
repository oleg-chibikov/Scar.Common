using Microsoft.AspNetCore.Mvc;

namespace Scar.Common.WebApi.Startup.Launcher
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        public int Add(int i = 0)
        {
            return i + 1;
        }
    }
}
