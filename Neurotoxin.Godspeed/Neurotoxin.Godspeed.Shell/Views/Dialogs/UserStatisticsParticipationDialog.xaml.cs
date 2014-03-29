using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Formatters;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Reporting;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class UserStatisticsParticipationDialog
    {
        public UserStatisticsParticipationDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }

        protected override void OkButtonClick(object sender, RoutedEventArgs e)
        {
            var p = No.IsChecked != true;
            UserSettings.DisableUserStatisticsParticipation = p;
            HttpForm.Post("clients.php", new List<IFormData>
                    {
                        new RawPostData("client_id", EsentPersistentDictionary.Instance.Get<string>("ClientID")),
                        new RawPostData("date", DateTime.Now.ToUnixTimestamp()),
                        new RawPostData("participates", p ? "yes" : "no"),
                    });
            base.OkButtonClick(sender, e);
        }
    }
}