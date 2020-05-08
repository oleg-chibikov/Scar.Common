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
                    var filePath = session.Property(InstallBuilder.CustomParam);
                    if (string.IsNullOrEmpty(filePath))
                    {
                        throw new InvalidOperationException("FilePath is empty");
                    }

                    if (!File.Exists(filePath))
                    {
                        throw new InvalidOperationException(filePath + " does not exist");
                    }

                    var externalTool = new ExternalTool { ExePath = filePath, Arguments = "install" };
                    session.Log($"Executing '{filePath} install'...");
                    externalTool.WinRun();
                    externalTool.Arguments = "start";
                    session.Log($"Executing '{filePath} start'...");
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
                    var installDir = session.Property("INSTALLDIR");
                    var fileName = session.Property(InstallBuilder.FileName);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        throw new InvalidOperationException("FileName is empty");
                    }

                    var filePath = Path.Combine(installDir, fileName);
                    if (!File.Exists(filePath))
                    {
                        throw new InvalidOperationException(filePath + " does not exist");
                    }

                    session.Log($"Executing '{filePath}'...");

                    using var process = new Process { StartInfo = new ProcessStartInfo(filePath) { UseShellExecute = true } };
                    process.Start();
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

                    using var process = new Process { StartInfo = new ProcessStartInfo(installDir) { UseShellExecute = true } };
                    process.Start();
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
                        throw new InvalidOperationException("ProcessName is empty");
                    }

                    const string command = @"taskkill";
                    var param = $@"/F /IM ""{processName}""";
                    session.Log($"Executing '{command} {param}'...");
                    var process = new Process { StartInfo = { FileName = command, Arguments = param, UseShellExecute = false, CreateNoWindow = true } };
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
                    var filePath = session.Property(InstallBuilder.CustomParam);
                    if (string.IsNullOrEmpty(filePath))
                    {
                        throw new InvalidOperationException("FilePath is empty");
                    }

                    if (!File.Exists(filePath))
                    {
                        throw new InvalidOperationException(filePath + " does not exist");
                    }

                    var externalTool = new ExternalTool { ExePath = filePath, Arguments = "uninstall" };
                    session.Log($"Executing '{filePath}' uninstall...");
                    externalTool.WinRun();
                    session.Log($"Deleting '{installDir}'...");
                    Directory.Delete(installDir, true);
                });
        }
    }
}
