using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class CacheMigrationViewModel : ViewModelBase
    {
        private int _itemsCount;
        private int _itemsMigrated;

        private const string PROGRESSVALUE = "ProgressValue";
        public int ProgressValue
        {
            get { return _itemsMigrated * 100 / _itemsCount; }
        }

        private const string PROGRESSVALUEDOUBLE = "ProgressValueDouble";
        public double ProgressValueDouble
        {
            get { return (double) ProgressValue/100; }
        }

        public event MigrationFinishedEventHandler MigrationFinished;

        private void NotifyMigrationFinished()
        {
            var handler = MigrationFinished;
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
                        NotifyPropertyChanged(PROGRESSVALUE);
                        NotifyPropertyChanged(PROGRESSVALUEDOUBLE);
                    }
                    return true;
                },
                b =>
                    {
                        e.MigrationFinishedAction();
                        NotifyMigrationFinished();
                    });
        }
    }
}