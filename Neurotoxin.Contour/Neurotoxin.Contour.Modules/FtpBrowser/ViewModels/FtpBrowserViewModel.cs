using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using FtpLib;
using Neurotoxin.Contour.Core;
using Neurotoxin.Contour.Core.Io.Stfs;
using Neurotoxin.Contour.Core.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using Neurotoxin.Contour.Presentation.ViewModels;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    /// <summary>
    /// ViewModel for FtpBrowserView.
    /// </summary>
    public class FtpBrowserViewModel : ModuleViewModelBase
    {
        private FtpConnection _ftpClient;

        #region Properties

        private const string LEFTCONTENT = "LeftContent";
        private ObservableCollection<FtpItem> _leftContent;
        public ObservableCollection<FtpItem> LeftContent
        {
            get { return _leftContent; }
            set { _leftContent = value; NotifyPropertyChanged(LEFTCONTENT); }
        }

        private const string LEFTSELECTION = "LeftSelection";
        private FtpItem _leftSelection;
        public FtpItem LeftSelection
        {
            get { return _leftSelection; }
            set { _leftSelection = value; NotifyPropertyChanged(LEFTCONTENT); }
        }

        #endregion

        public override bool HasDirty()
        {
            throw new NotImplementedException();
        }

        protected override void ResetDirtyFlags()
        {
            throw new NotImplementedException();
        }

        public override bool IsDirty(string propertyName)
        {
            throw new NotImplementedException();
        }

        #region Commands

        public DelegateCommand<object> ChangeDirectoryCommand { get; private set; }

        private void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            LoadSubscribe();
            //WorkerThread.Run(ChangeDirectory, ChangeDirectoryCallback);
        }

        private bool CanExecuteChangeDirectoryCommand(object cmdParam)
        {
            //UNDONE
            return true;
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
        }

        #endregion

        public FtpBrowserViewModel()
        {
            ChangeDirectoryCommand = new DelegateCommand<object>(ExecuteChangeDirectoryCommand, CanExecuteChangeDirectoryCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    LoadSubscribe();
                    WorkerThread.Run(Connect, ConnectCallback);
                    break;
            }
        }

        private void LoadSubscribe()
        {
            IsInProgress = true;
            LoadingQueueLength = 1;
            LoadingProgress = 0;
            LogHelper.StatusBarChange += LogHelperStatusBarChange;
            LogHelper.StatusBarMax += LogHelperStatusBarMax;
            LogHelper.StatusBarText += LogHelperStatusBarText;
        }

        private void LogHelperStatusBarChange(object sender, ValueChangedEventArgs e)
        {
            UIThread.BeginRun(() => LoadingProgress = e.NewValue);
        }

        private void LogHelperStatusBarMax(object sender, ValueChangedEventArgs e)
        {
            UIThread.BeginRun(() =>
                                  {
                                      LoadingQueueLength = e.NewValue;
                                      LoadingProgress = 0;
                                  });
        }

        private void LogHelperStatusBarText(object sender, TextChangedEventArgs e)
        {
            UIThread.BeginRun(() => LoadingInfo = e.Text);
        }

        private ObservableCollection<FtpItem> Connect()
        {
            var baseDir = "/Hdd1/Content/";
            _ftpClient = new FtpConnection("192.168.1.110","xbox", "xbox");
            _ftpClient.Open();
            _ftpClient.Login();
            _ftpClient.SetCurrentDirectory(baseDir);
            var content = new ObservableCollection<FtpItem>();
            if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
            foreach(var di in _ftpClient.GetDirectories())
            {
                var path = string.Format("{1}{0}/FFFE07D1/00010000/{0}", di.Name, baseDir);
                var tmpPath = string.Format("tmp/{0}", di.Name);
                if (!_ftpClient.FileExists(path)) continue;
                _ftpClient.GetFile(path, tmpPath, false); // throws exception!!!
                var stfs = ModelFactory.GetModel<StfsPackage>(tmpPath);
                content.Add(new FtpItem
                                {
                                    TitleId = di.Name,
                                    Title = stfs.DisplayName
                                });
            }
            return content;
        }

        private void ConnectCallback(ObservableCollection<FtpItem> content)
        {
            LeftContent = content;
            IsInProgress = false;
            LogHelper.StatusBarChange -= LogHelperStatusBarChange;
            LogHelper.StatusBarMax -= LogHelperStatusBarMax;
            LogHelper.StatusBarText -= LogHelperStatusBarText;
            LoadingInfo = "Done.";
        }

    }
}