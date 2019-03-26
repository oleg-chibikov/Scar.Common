using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;
using Scar.Common.MVVM.Commands;
using Scar.Common.MVVM.ViewModel;
using WixSharp.UI;

namespace Scar.Common.Installer.UI.ViewModel
{
    public sealed class SetupViewModel : GenericSetup, IRequestCloseViewModel
    {
        private string _installPath;

        public SetupViewModel([NotNull] string msiFile, bool enableLogging = true)
            : base(msiFile, enableLogging)
        {
            _ = msiFile ?? throw new ArgumentNullException(nameof(msiFile));
            InitialCanInstall = CanInstall;
            InitialCanUnInstall = CanUnInstall;
            InitialCanRepair = CanRepair;
            var commandManager = new ApplicationCommandManager(SynchronizationContext.Current);
            InstallCommand = new CorrelationCommand<string>(commandManager, Install);
            UninstallCommand = new CorrelationCommand(commandManager, Uninstall);
            RepairCommand = new CorrelationCommand(commandManager, Repair);
            CancelCommand = new CorrelationCommand(commandManager, Cancel);
            ShowLogCommand = new CorrelationCommand(commandManager, ShowLog);
            BrowseInstallationPathCommand = new CorrelationCommand(commandManager, BrowseInstallationPath);

            var msiParser = new MsiParser(MsiFile);
            InstallPath = msiParser.GetDirectoryPath("INSTALLDIR");

            // ReSharper disable once AssignNullToNotNullAttribute
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            CompanyName = versionInfo.CompanyName;
            Year = DateTime.Now.Year;

            //Uncomment if you want to see current action name changes. Otherwise it is too quick.
            //ProgressStepDelay = 500;
        }

        public int Year { get; set; }

        [NotNull]
        public string CompanyName { get; set; }

        public bool InitialCanInstall { get; set; }

        public bool InitialCanUnInstall { get; set; }

        public bool InitialCanRepair { get; set; }

        [CanBeNull]
        public string InstallPath
        {
            get => _installPath;
            set
            {
                _installPath = value;
                OnPropertyChanged(nameof(InstallPath));
            }
        }

        [NotNull]
        public ICommand InstallCommand { get; }

        [NotNull]
        public ICommand UninstallCommand { get; }

        [NotNull]
        public ICommand RepairCommand { get; }

        [NotNull]
        public ICommand CancelCommand { get; }

        [NotNull]
        public ICommand ShowLogCommand { get; }

        [NotNull]
        public ICommand BrowseInstallationPathCommand { get; }

        public event EventHandler RequestClose;

        private void BrowseInstallationPath()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                InstallPath = dialog.FileName;
            }
        }

        private void Cancel()
        {
            if (IsRunning)
            {
                CancelRequested = true;
            }
            else
            {
                RequestClose?.Invoke(null, null);
            }
        }

        private void Install([CanBeNull] string path)
        {
            string pathParam = null;
            if (!string.IsNullOrEmpty(path))
            {
                if (!Path.IsPathRooted(path))
                {
                    ErrorStatus = $"The path {path} is not valid";
                    return;
                }

                pathParam = $"INSTALLDIR=\"{path}\"";
            }

            StartInstall(pathParam);
        }

        private void Repair()
        {
            StartRepair();
        }

        private void ShowLog()
        {
            Process.Start(LogFile);
        }

        private void Uninstall()
        {
            StartUninstall();
        }
    }
}