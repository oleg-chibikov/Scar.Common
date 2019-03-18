using System;
using System.Diagnostics;
using System.IO;
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

        public const string FileName = nameof(FileName);

        [NotNull]
        private const string DesktopPath = "%Desktop%";

        [NotNull]
        private readonly string _exeFileName;

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
            _exeFileName = _productName + ".exe";
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
                    new Property("ALLUSERS", "1"),
                    new Property(FileName, _exeFileName)
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

        public void Build([CanBeNull] string outputMsiPath = null, [CanBeNull] string wixBinariesLocation = null)
        {
            if (wixBinariesLocation != null)
            {
                Compiler.WixLocation = wixBinariesLocation;
            }

            if (outputMsiPath == null)
            {
                outputMsiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _productName + ".msi");
            }

            Compiler.BuildMsi(_project, outputMsiPath);
        }

        [NotNull]
        public InstallBuilder LaunchAfterInstallation()
        {
            _project.AddAction(new ManagedAction(CustomActions.LaunchProcess, Return.check, When.After, Step.InstallFinalize, Condition.NOT_Installed));
            return this;
        }

        [NotNull]
        public InstallBuilder OpenFolderAfterInstallation()
        {
            _project.AddAction(new ManagedAction(CustomActions.OpenFolder, Return.check, When.After, Step.InstallFinalize, Condition.NOT_Installed));
            return this;
        }

        [NotNull]
        public InstallBuilder WithAutostart([CanBeNull] string fileName = null)
        {
            _project.AddRegValue(
                new RegValue(
                    new Id(_productName + "Autostart"),
                    RegistryHive.LocalMachine,
                    @"Software\Microsoft\Windows\CurrentVersion\Run",
                    _productName,
                    $"[INSTALLDIR]{fileName ?? _exeFileName}"));
            return this;
        }

        [NotNull]
        public InstallBuilder WithIcon([NotNull] string iconFile)
        {
            _project.ControlPanelInfo.ProductIcon = iconFile ?? throw new ArgumentNullException(nameof(iconFile));
            return this;
        }

        [NotNull]
        public InstallBuilder WithProcessTermination([CanBeNull] string fileName = null)
        {
            var customActionParam = $"{CustomParam}={fileName ?? _exeFileName}";
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
        public InstallBuilder WithDesktopShortcut([CanBeNull] string fileName = null, [CanBeNull] string arguments = "")
        {
            var formattableString = $"[INSTALLDIR]{fileName ?? _exeFileName}";
            var dir = _project.FindDir(DesktopPath);
            var shortcut = new ExeFileShortcut(_productName, formattableString, arguments);
            dir.Shortcuts = dir.Shortcuts.Combine(shortcut);
            return this;
        }

        [NotNull]
        public InstallBuilder WithProgramMenuShortcut([CanBeNull] string fileName = null, [CanBeNull] string arguments = "")
        {
            var formattableString = $"[INSTALLDIR]{fileName ?? _exeFileName}";
            var dir = _project.FindDir(_programMenuPath);
            var shortcut = new ExeFileShortcut(_productName, formattableString, arguments);
            dir.Shortcuts = dir.Shortcuts.Combine(shortcut);
            return this;
        }

        [NotNull]
        public InstallBuilder WithWinService([CanBeNull] string fileName = null)
        {
            var customActionParam = $"{CustomParam}=[INSTALLDIR]{fileName ?? _exeFileName}";
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