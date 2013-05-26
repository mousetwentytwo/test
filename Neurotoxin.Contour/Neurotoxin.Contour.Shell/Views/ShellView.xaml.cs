using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Neurotoxin.Contour.Presentation.Controls;
using Neurotoxin.Contour.Presentation.Events;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Shell.Constants;
using Neurotoxin.Contour.Shell.Controllers;
using Neurotoxin.Contour.Shell.ViewModels;

namespace Neurotoxin.Contour.Shell.Views
{
    public partial class ShellView : Window
    {
        internal const string LAYOUTKEY = "AvalonDock.Layout";

        #region Private fields

        private readonly IUnityContainer container;
        private readonly IModuleManager moduleManager;
        private readonly IModuleCatalog moduleCatalog;
        private readonly ModuleController moduleController;
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;
        private readonly TabController tabController;

        #endregion

        #region Constructor

        public ShellView(ShellViewModel viewModel, IUnityContainer container, IModuleManager moduleManager, IModuleCatalog moduleCatalog, IEventAggregator eventAggregator, IRegionManager regionManager, ModuleController moduleController, TabController tabController)
        {
            this.container = container;
            this.moduleManager = moduleManager;
            this.moduleCatalog = moduleCatalog;
            this.moduleController = moduleController;
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.tabController = tabController;
            InitializeComponent();

            // Set the ViewModel as this View's data context.
            this.DataContext = viewModel;
            this.Loaded += OnShellViewLoaded;

            eventAggregator.GetEvent<ModuleOpenEvent>().Subscribe(OpenTab, ThreadOption.UIThread, true);
            eventAggregator.GetEvent<ModuleCloseEvent>().Subscribe(CloseTab, ThreadOption.UIThread, true);
            eventAggregator.GetEvent<ExceptionEvent>().Subscribe(HandleException, ThreadOption.UIThread, true);
            //eventAggregator.GetEvent<LogEvent>().Subscribe(TraceLog, ThreadOption.BackgroundThread, true);
            eventAggregator.GetEvent<InitializeGesturesAndCommandsEvent>().Subscribe(InitializeGesturesAndCommands, ThreadOption.UIThread, true);
            //eventAggregator.GetEvent<SaveLayoutEvent>().Subscribe(SaveLayout, ThreadOption.UIThread, true);
            //eventAggregator.GetEvent<RestoreLayoutEvent>().Subscribe(RestoreLayout, ThreadOption.UIThread, true);
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Occurs when the ShellView has been loaded and sets the ItemsSource of the TabControl.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShellViewLoaded(object sender, RoutedEventArgs e)
        {
            ShellViewModel viewModel = this.DataContext as ShellViewModel;
            if (viewModel == null) return;

            viewModel.Initialize();

            //HACK zsbangha 2010-05-28: it's not very MVVMish :S
            tabController.ShellView = this;
            tabController.DefaultDocumentPane = Tabs;

            var mli = ModuleLoadInfoCollection.ProfileEditor.Clone();
            mli.LoadParameter = @"..\..\..\..\Resources\mergeable\aktualis\E00001D5D85ED487.orig";
            //mli.LoadParameter = @"..\..\..\..\Resources\mergeable\aktualis\merge.1x";
            //mli.LoadParameter = @"..\..\..\..\Resources\mergeable\aktualis\take8.base";
            //mli.LoadParameter = @"..\..\..\..\Resources\mergeable\E0000027FA233BE2";
            tabController.AddItem(mli);

            this.Loaded -= OnShellViewLoaded;
        }

        /// <summary>
        /// Occurs when the DataContext of the TabControl object changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabsDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Tabs.Items.Count == 0) return;
            //this.Tabs.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles Tab changes. Loads and shows specified module.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            foreach (object o in e.RemovedItems)
            {
                var item = o as DockableTabItem;
                if (item == null || item.LoadInfo.RenderedView == null) continue;
                item.LoadInfo.RenderedView.IsActive = false;
            }
            foreach (object o in e.AddedItems)
            {
                DockableTabItem item = o as DockableTabItem;
                if (item == null || item.LoadInfo.RenderedView == null) continue;
                item.LoadInfo.RenderedView.IsActive = true;
            }
        }

        #endregion

        #region Methods

        // public void ShowTabContent(Common.Controls.DockableTabItem item)
        // {
        //     if (item == null || moduleController == null) return;

        //     ModuleLoadInfo info = item.LoadInfo;
        //     if (info == null) return;
        //}

        public void OpenTab(ModuleOpenEventArgs args)
        {
            tabController.AddItem(args.LoadInfo);
            tabController.SelectModule(args.LoadInfo);
        }

        public void CloseTab(ModuleCloseEventArgs args)
        {
            tabController.CloseTab(args.LoadInfo, args.Force);
        }

        public void InitializeGesturesAndCommands(MainMenuBindings bindings)
        {
            this.CommandBindings.Clear();
            this.CommandBindings.AddRange(bindings.CommandBindings);
            this.InputBindings.Clear();
            this.InputBindings.AddRange(bindings.InputBindings);
        }

        public void HandleException(Exception ex)
        {
            //GlobalModel.TryToReportExceptionToServerInNewThread(ex);
            //Logger.Write(ex, Category.Exception.ToString(), (int)Priority.None);
            //ExceptionHandler.ShowMessage(ex);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            //HACK 2010-05-31
            ShellViewModel viewModel = (ShellViewModel)this.DataContext;
            if (tabController.Modules != null)
            {
                foreach (ModuleLoadInfo info in tabController.Modules)
                {
                    if (info.RenderedView == null) continue;
                    if (!info.RenderedView.Close())
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            if (e.Cancel) return;
            eventAggregator.GetEvent<ModuleCloseEvent>().Unsubscribe(CloseTab);
            eventAggregator.GetEvent<ExceptionEvent>().Unsubscribe(HandleException);
            eventAggregator.GetEvent<InitializeGesturesAndCommandsEvent>().Unsubscribe(InitializeGesturesAndCommands);
            //eventAggregator.GetEvent<SaveLayoutEvent>().Unsubscribe(SaveLayout);
            //eventAggregator.GetEvent<RestoreLayoutEvent>().Unsubscribe(RestoreLayout);
        }

        #endregion
    }
}