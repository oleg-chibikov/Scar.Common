using System;
using Microsoft.AspNetCore.Mvc;

namespace Scar.Common.WebApi.Startup.Launcher.WebApi.Startup.Launcher;

[Route("api/[controller]")]
public class TestController : Controller
{
    readonly Dependency _dependency;

    public TestController(Dependency dependency)
    {
        _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
    }

    [HttpGet]
    public int Add(int i = 0)
    {
        _dependency.Method();
        return i + 1;
    }
}
