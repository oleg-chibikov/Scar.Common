using System;
using System.Diagnostics;
using JetBrains.Annotations;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Forms;
using Action = WixSharp.Action;
using Assembly = System.Reflection.Assembly;

namespace Scar.Common.Installer
{
    public class InstallBuilder
    {
        [NotNull]
        public const string CustomParam = nameof(CustomParam);

        [NotNull]
        private const string DesktopPath = "%Desktop%";

        [NotNull]
        private readonly string _productName;

        [NotNull]
        private readonly string _programMenuPath;

        [NotNull]
        private readonly ManagedProject _project;

        public InstallBuilder([NotNull] string productName, [CanBeNull] string companyName, [NotNull] string buildDir, Guid upgradeCode)
        {
            if (buildDir == null)
            {
                throw new ArgumentNullException(nameof(buildDir));
            }

            _productName = productName ?? throw new ArgumentNullException(nameof(productName));
            var assembly = Assembly.GetEntryAssembly();
            var assemblyName = assembly.GetName();
            var version = assemblyName.Version;

            // ReSharper disable once AssignNullToNotNullAttribute
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var uninstallText = $"Uninstall {productName}";
            const string system64FolderMsiexecExe = "[System64Folder]msiexec.exe";
            const string productCode = "/x [ProductCode]";
            _programMenuPath = $@"%ProgramMenu%\{productName}";
            var programMenuDir = new Dir(_programMenuPath, new ExeFileShortcut(uninstallText, system64FolderMsiexecExe, productCode));
            var desktopDir = new Dir(DesktopPath);
            companyName = companyName == null ? null : $"{companyName}\\";
            _project = new ManagedProject(productName)
            {
                Dirs = new[]
                {
                    new Dir(
                        $@"%ProgramFiles%\{companyName}{productName}",
                        new Files($@"{buildDir}\*.*"),
                        new ExeFileShortcut(uninstallText, system64FolderMsiexecExe, productCode)),
                    programMenuDir,
                    desktopDir
                },
                Properties = new[]
                {
                    new Property("ALLUSERS", "1")
                },
                UpgradeCode = upgradeCode,
                Version = version,
                MajorUpgrade = new MajorUpgrade
                {
                    AllowSameVersionUpgrades = false,
                    DowngradeErrorMessage = "A later version of [ProductName] is already installed",
                    Schedule = UpgradeSchedule.afterInstallInitialize
                },
                MajorUpgradeStrategy = new MajorUpgradeStrategy
                {
                    RemoveExistingProductAfter = Step.InstallInitialize,
                    UpgradeVersions = VersionRange.ThisAndOlder
                },
                ManagedUI = new ManagedUI
                {
                    InstallDialogs = new ManagedDialogs
                    {
                        Dialogs.Welcome,
                        Dialogs.InstallDir,
                        Dialogs.Progress,
                        Dialogs.Exit
                    },
                    ModifyDialogs = new ManagedDialogs
                    {
                        Dialogs.MaintenanceType,
                        Dialogs.Progress,
                        Dialogs.Exit
                    }
                },
                ControlPanelInfo =
                {
                    Manufacturer = versionInfo.CompanyName
                }
            };
        }

        public void Build([CanBeNull] string outputMsiPath = null)
        {
            Compiler.BuildMsi(_project, outputMsiPath);
        }

        [NotNull]
        public InstallBuilder LaunchAfterInstallation([NotNull] string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var customActionParam = $"{CustomParam}=[INSTALLDIR]{fileName}";
            _project.AddAction(
                new ElevatedManagedAction(CustomActions.LaunchProcess, Return.check, When.Before, Step.InstallFinalize, Condition.NOT_Installed)
                {
                    UsesProperties = customActionParam
                });
            return this;
        }

        [NotNull]
        public InstallBuilder OpenFolderAfterInstallation()
        {
            _project.AddAction(new ManagedAction(CustomActions.OpenFolder, Return.check, When.After, Step.InstallFinalize, Condition.NOT_Installed));
            return this;
        }

        [NotNull]
        public InstallBuilder WithAutostart([NotNull] string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            _project.AddRegValue(
                new RegValue(
                    new Id(_productName + "Autostart"),
                    RegistryHive.LocalMachine,
                    @"Software\Microsoft\Windows\CurrentVersion\Run",
                    _productName,
                    $"[INSTALLDIR]{fileName}"));
            return this;
        }

        [NotNull]
        public InstallBuilder WithIcon([NotNull] string iconFile)
        {
            _project.ControlPanelInfo.ProductIcon = iconFile ?? throw new ArgumentNullException(nameof(iconFile));
            return this;
        }

        [NotNull]
        public InstallBuilder WithProcessTermination([NotNull] string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var customActionParam = $"{CustomParam}={fileName}";
            _project.AddAction(
                new ElevatedManagedAction(CustomActions.TerminateProcess, Return.check, When.Before, Step.InstallFiles, Condition.NOT_Installed)
                {
                    UsesProperties = customActionParam
                });
            _project.AddAction(
                new ElevatedManagedAction(CustomActions.TerminateProcess, Return.check, When.Before, Step.RemoveFiles, Condition.BeingUninstalled)
                {
                    UsesProperties = customActionParam
                });
            return this;
        }

        [NotNull]
        public InstallBuilder WithDesktopShortcut([NotNull] string fileName, [CanBeNull] string arguments = "")
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var formattableString = $"[INSTALLDIR]{fileName}";
            var dir = _project.FindDir(DesktopPath);
            var shortcut = new ExeFileShortcut(_productName, formattableString, arguments);
            dir.Shortcuts = dir.Shortcuts.Combine(shortcut);
            return this;
        }

        [NotNull]
        public InstallBuilder WithProgramMenuShortcut([NotNull] string fileName, [CanBeNull] string arguments = "")
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var formattableString = $"[INSTALLDIR]{fileName}";
            var dir = _project.FindDir(_programMenuPath);
            var shortcut = new ExeFileShortcut(_productName, formattableString, arguments);
            dir.Shortcuts = dir.Shortcuts.Combine(shortcut);
            return this;
        }

        [NotNull]
        public InstallBuilder WithWinService([NotNull] string serviceExecutableName)
        {
            if (serviceExecutableName == null)
            {
                throw new ArgumentNullException(nameof(serviceExecutableName));
            }

            var customActionParam = $"{CustomParam}=[INSTALLDIR]{serviceExecutableName}";
            _project.Actions = new Action[]
            {
                new ElevatedManagedAction(CustomActions.InstallService, Return.check, When.After, Step.InstallFiles, Condition.NOT_Installed)
                {
                    UsesProperties = customActionParam
                },
                new ElevatedManagedAction(CustomActions.UninstallService, Return.check, When.Before, Step.RemoveFiles, Condition.BeingUninstalled)
                {
                    UsesProperties = customActionParam
                }
            };
            return this;
        }
    }
}