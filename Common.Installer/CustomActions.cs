using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.CommonTasks;
using File = System.IO.File;

namespace Scar.Common.Installer
{
    public static class CustomActions
    {
        [CustomAction]
        public static ActionResult InstallService(Session session)
        {
            return session.HandleErrors(
                () =>
                {
                    // Debugger.Launch();
                    var path = session.Property(InstallBuilder.CustomParam);
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new ArgumentNullException(nameof(path));
                    }

                    if (!File.Exists(path))
                    {
                        return;
                    }

                    var externalTool = new ExternalTool
                    {
                        ExePath = path,
                        Arguments = "install"
                    };
                    session.Log($"Executing '{path} install'...");
                    externalTool.WinRun();
                    externalTool.Arguments = "start";
                    session.Log($"Executing '{path} start'...");
                    externalTool.WinRun();
                });
        }

        [CustomAction]
        public static ActionResult LaunchProcess(Session session)
        {
            return session.HandleErrors(
                () =>
                {
                    // Debugger.Launch();
                    var path = session.Property(InstallBuilder.CustomParam);
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new ArgumentNullException(nameof(path));
                    }

                    if (!File.Exists(path))
                    {
                        return;
                    }

                    session.Log($"Executing '{path}'...");
                    Process.Start(path);
                });
        }

        [CustomAction]
        public static ActionResult OpenFolder(Session session)
        {
            return session.HandleErrors(
                () =>
                {
                    // Debugger.Launch();
                    var installDir = session.Property("INSTALLDIR");
                    session.Log($"Executing '{installDir}'...");
                    Process.Start(installDir);
                });
        }

        [CustomAction]
        public static ActionResult TerminateProcess(Session session)
        {
            return session.HandleErrors(
                () =>
                {
                    // Debugger.Launch();
                    var processName = session.Property(InstallBuilder.CustomParam);
                    if (string.IsNullOrEmpty(processName))
                    {
                        throw new ArgumentNullException(nameof(processName));
                    }

                    const string command = @"taskkill";
                    var param = $@"/F /IM ""{processName}""";
                    session.Log($"Executing '{command} {param}'...");
                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = command,
                            Arguments = param,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                });
        }

        [CustomAction]
        public static ActionResult UninstallService(Session session)
        {
            return session.HandleErrors(
                () =>
                {
                    // Debugger.Launch();
                    var installDir = session.Property("INSTALLDIR");
                    var path = session.Property(InstallBuilder.CustomParam);
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new ArgumentNullException(nameof(path));
                    }

                    if (!File.Exists(path))
                    {
                        return;
                    }

                    var externalTool = new ExternalTool
                    {
                        ExePath = path,
                        Arguments = "uninstall"
                    };
                    session.Log($"Executing '{path}' uninstall...");
                    externalTool.WinRun();
                    session.Log($"Deleting '{installDir}'...");
                    Directory.Delete(installDir, true);
                });
        }
    }
}