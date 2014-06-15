using System;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Helpers;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FtpTraceViewModel : ViewModelBase
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly FtpTraceListener _traceListener;

        private const string LOG = "Log";
        private string _log;
        public string Log
        {
            get { return _log; }
            set { _log = value; NotifyPropertyChanged(LOG); }
        }

        private const string TITLE = "Title";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        #region CloseCommand

        public DelegateCommand CloseCommand { get; private set; }

        private void ExecuteCloseCommand()
        {
            EventAggregator.GetEvent<CloseFtpTraceWindowEvent>().Publish(new CloseFtpTraceWindowEventArgs(_traceListener));
        }

        #endregion

        public FtpTraceViewModel(FtpTraceListener traceListener, string connectionName)
        {
            _traceListener = traceListener;
            var log = traceListener.Log;
            for (var i = log.Count - 1; i >= 0; i--)
            {
                _stringBuilder.Append(log.ElementAt(i));
            }
            Log = _stringBuilder.ToString();
            Title = string.Format(Resx.FtpTraceWindowTitle, connectionName);
            _traceListener.LogChanged += TraceListenerOnLogChanged;

            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
        }

        private void TraceListenerOnLogChanged(object sender, LogChangedEventArgs e)
        {
            _stringBuilder.Append(e.Message);
            Log = _stringBuilder.ToString();
        }

        public override void Dispose()
        {
            base.Dispose();
            _traceListener.LogChanged -= TraceListenerOnLogChanged;
        }
    }
}