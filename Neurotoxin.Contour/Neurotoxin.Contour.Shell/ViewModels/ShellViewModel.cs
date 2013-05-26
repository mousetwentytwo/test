using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Regions;
using Neurotoxin.Contour.Core;
using Neurotoxin.Contour.Presentation.Events;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using Neurotoxin.Contour.Shell.Controllers;

namespace Neurotoxin.Contour.Shell.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        #region String constants

        internal const string MODULES = "Modules";
        internal const string MENUITEMS = "MenuItems";
        internal const string ERROR = "Error";
        internal const string WARNING = "Warning";
        internal const string APPLICATIONTITLE = "ApplicationTitle";

        #endregion

        #region Private fields

        private readonly IRegionManager regionManager;
        private readonly TabController tabController;
        private readonly IEventAggregator eventAggregator;
        //private List<MenuItem> menuItems;

        #endregion

        #region Public properties

        private Exception error;
        public Exception Error
        {
            get { return error; }
            set { error = value; NotifyPropertyChanged(ERROR); }
        }

        private Exception warning;
        public Exception Warning
        {
            get { return warning; }
            set { warning = value; NotifyPropertyChanged(WARNING); }
        }

        private string _applicationTitle;
        public string ApplicationTitle
        {
            get { return _applicationTitle; }
            set { _applicationTitle = value; NotifyPropertyChanged(APPLICATIONTITLE); }
        }

        #endregion

        public ShellViewModel(IRegionManager regionManager, TabController tabController, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.tabController = tabController;
            this.eventAggregator = eventAggregator;

            Assembly assembly = Assembly.GetAssembly(typeof(ShellViewModel));
            AssemblyName assemblyName = assembly.GetName();
            Version version = assemblyName.Version;
            ApplicationTitle = String.Format("CONtour v{0}", version);


            //UnityInstance.Container.Resolve<ShellClosedEvent>().Subscribe(OnShellClosed, ThreadOption.PublisherThread, false);
        }

        //private void OnShellClosed(object dummy)
        //{
        //    UnityInstance.Container.Resolve<ShellClosedEvent>().Unsubscribe(OnShellClosed);
        //}

        public void Initialize()
        {
        }
    }
}