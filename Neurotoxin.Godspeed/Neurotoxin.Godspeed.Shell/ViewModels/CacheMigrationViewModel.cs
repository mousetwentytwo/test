using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class CacheMigrationViewModel : ViewModelBase, IProgressViewModel
    {
        private int _itemsCount;
        private int _itemsMigrated;

        private const string PROGRESSDIALOGTITLE = "ProgressDialogTitle";
        public string ProgressDialogTitle
        {
            get { return string.Format(Resx.MigrationProgressDialogTitle, ProgressValue); }
        }

        private const string PROGRESSMESSAGE = "ProgressMessage";
        public string ProgressMessage
        {
            get { return Resx.MigrationProgressMessage; }
        }

        private const string PROGRESSVALUE = "ProgressValue";
        public int ProgressValue
        {
            get { return _itemsCount == 0 ? 0 : _itemsMigrated * 100 / _itemsCount; }
        }

        private const string PROGRESSVALUEDOUBLE = "ProgressValueDouble";
        public double ProgressValueDouble
        {
            get { return (double) ProgressValue/100; }
        }

        private const string ISINDETERMINE = "IsIndetermine";
        public bool IsIndetermine
        {
            get { return false; }
        }

        public event WorkFinishedEventHandler Finished;

        private void NotifyFinished()
        {
            var handler = Finished;
            if (handler != null) handler.Invoke(this);
        }

        public void Initialize(CacheMigrationEventArgs e)
        {
            _itemsCount = e.Items.Count;
            WorkerThread.Run(() =>
                {
                    foreach (var item in e.Items)
                    {
                        e.ItemMigrationAction.Invoke(item.Key, item.Value, e.CacheStore, e.Payload);
                        _itemsMigrated++;
                        UIThread.Run(() =>
                        {
                            NotifyPropertyChanged(PROGRESSVALUE);
                            NotifyPropertyChanged(PROGRESSVALUEDOUBLE);
                            NotifyPropertyChanged(PROGRESSDIALOGTITLE);
                        });
                    }
                    return true;
                },
                b =>
                    {
                        e.MigrationFinishedAction();
                        NotifyFinished();
                    });
        }
    }
}