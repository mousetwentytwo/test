using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using AvalonDock;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Regions;
using System.Globalization;
using Neurotoxin.Contour.Presentation.Controls;
using Neurotoxin.Contour.Presentation.Events;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Shell.Views;
using UIThread = Neurotoxin.Contour.Presentation.Infrastructure.UIThread;

namespace Neurotoxin.Contour.Shell.Controllers
{
    /// <summary>
    /// Converter class to the ShellView's TabControl. This generates TabItems from the ModuleLoadInfo instances.
    /// </summary>
    public class TabController : IValueConverter, ITabController
    {
        #region Private fields

        private readonly IRegionManager regionManager;
        private readonly IUnityContainer container;
        private readonly IEventAggregator eventAggregator;
        private readonly ModuleController moduleController;
        private List<Tuple<ModuleLoadInfo, DockableTabItem>> tabItems = new List<Tuple<ModuleLoadInfo, DockableTabItem>>();

        #endregion

        #region Properties

        public ShellView ShellView { get; set; }
        public DocumentPane DefaultDocumentPane { get; set; }

        public List<ModuleLoadInfo> Modules
        {
            get 
            {
                return tabItems.Select(s=>s.Item1).ToList();
            }
        }

        public List<DockableTabItem> Tabs
        {
            get
            {
                return tabItems.Select(s => s.Item2).ToList();
            }
        }

        public ModuleLoadInfo SelectedModule
        {
            get
            {
                if (DefaultDocumentPane == null) return null;
                DockableTabItem item = DefaultDocumentPane.SelectedItem as DockableTabItem;
                if (item == null) return null;
                return item.LoadInfo;
            }
            set
            {
                if (!tabItems.Any(s=>s.Item1.Equals(value))) AddItem(value);
                BringMLIToFront(value);
            }
        }

        public ModuleViewBase SelectedTabView
        {
            get 
            {
                ModuleLoadInfo mli = SelectedModule;
                if (mli == null) return null;
                return mli.RenderedView;
            }
        }

        #endregion

        #region Constructor

        public TabController(IUnityContainer container, IEventAggregator eventAggregator, IRegionManager regionManager, ModuleController moduleController)
        {
            this.regionManager = regionManager;
            this.container = container;
            this.eventAggregator = eventAggregator;
            this.moduleController = moduleController;

            //eventAggregator.GetEvent<ResetToDefaultsEvent>().Subscribe(ResetToDefaults);
            //eventAggregator.GetEvent<ModuleChangeEvent>().Subscribe(SelectModule, ThreadOption.UIThread, true);
            //eventAggregator.GetEvent<NewSplitModuleEvent>().Subscribe(NewSplitModule, ThreadOption.UIThread, true);
        }

        #endregion        

        /// <summary>
        /// Creates a new DockableTabItem instance
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private DockableTabItem CreateNewTab(ModuleLoadInfo info)
        {
            try
            {
                return new DockableTabItem(info, this);
            } 
            catch (Exception ex)
            {
                eventAggregator.GetEvent<ExceptionEvent>().Publish(ex);
            }
            return null;
        }

        /// <summary>
        /// Sets the Items collection property of the default DocumentPane
        /// </summary>
        /// <param name="items"></param>
        public void SetItems(List<ModuleLoadInfo> items)
        {
            SetItems(items, DefaultDocumentPane);
        }

        /// <summary>
        /// Sets the Items collection property of the given DocumentPane
        /// </summary>
        /// <param name="items"></param>
        /// <param name="target"></param>
        public void SetItems(List<ModuleLoadInfo> items, DocumentPane target)
        {
            object oTabs = this.Convert(items, typeof(DockableTabItem), null, CultureInfo.CurrentCulture);
            if (oTabs == null) throw new ArgumentNullException("TabController contains no elements!");

            //Moving every DocumentContent under the default DocumentPane, and gathering every unnecessary container panes
            List<DocumentContent> lTabs = (List<DocumentContent>)oTabs;
            target.Items.Clear();
            List<Pane> alternativePanes = new List<Pane>();
            foreach (DocumentContent tabItem in lTabs)
            {
                if (tabItem.ContainerPane != null && tabItem.ContainerPane != target)
                {
                    if (!alternativePanes.Contains(tabItem.ContainerPane))
                    {
                        alternativePanes.Add(tabItem.ContainerPane);
                        tabItem.ContainerPane.Items.Clear();
                    }
                }
                target.Items.Add(tabItem);
            }
            //Removing the unnecessary panes from the visual tree
            foreach (Pane pane in alternativePanes)
            {
                RemovePane(pane);
            }
            //Removing the unnecessary ResizingPanels from the DockingManager's content
            DockingManager manager = target.GetManager();
            ResizingPanel parent = (ResizingPanel)target.Parent;
            ResizingPanel content = (ResizingPanel)manager.Content;
            if (!Equals(parent, content))
            {
                parent.Children.Remove(target);
                int innerIndex =
                    content.Children.ToList<UIElement>().IndexOf(
                        w => !(w is DockablePane) && !(w is ResizingPanelSplitter));
                content.Children.RemoveAt(innerIndex);
                content.Children.Insert(innerIndex, target);
            }
            //Clearing the FloatingWindows array
            Array.Clear(manager.FloatingWindows, 0, manager.FloatingWindows.Length);
        }

        private void RemovePane(Pane pane)
        {
            DocumentPaneResizingPanel parentPanel = pane.Parent as DocumentPaneResizingPanel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(pane);
                return;
            }

            DocumentFloatingWindow parentWindow = pane.Parent as DocumentFloatingWindow;
            if (parentWindow != null && !parentWindow.IsClosing)
            {
                parentWindow.Close(true);
                parentWindow = null;
            }
        }

        /// <summary>
        /// Sets a Tab active with the given ModuleLoadInfo instance in the ShellView's TabControl.
        /// </summary>
        /// <param name="info">Module descriptor</param>
        public void SelectModule(ModuleLoadInfo info)
        {
            this.SelectedModule = info;
        }

        ///// <summary>
        ///// Creates two new modules and displays them in a splitted view.
        ///// </summary>
        //public void NewSplitModule(NewSplitModuleEventArgs e)
        //{
        //    SelectModule(e.Source);
        //    SelectModule(e.Target);
        //    if (e.Orientation == Orientation.Horizontal)
        //    {
        //        DefaultDocumentPane.NewHorizontalTabGroup();
        //    }
        //    else
        //    {
        //        DefaultDocumentPane.NewVerticalTabGroup();
        //    }
        //}

        /// <summary>
        /// Removes the module from the Tabs.
        /// </summary>
        /// <param name="moduleInfo"></param>
        public void RemoveModule(ModuleLoadInfo moduleInfo)
        {
            IRegion region = regionManager.Regions[moduleInfo.RegionName];

            //Remove view if exists
            IView currentView = (ModuleViewBase)region.GetView(moduleInfo.ViewName ?? moduleInfo.ModuleName);
            if (currentView != null) region.Remove(currentView);

            region = regionManager.Regions[moduleInfo.RegionName+ "StatusBar"];
            object stbview=region.GetView((moduleInfo.ViewName ?? moduleInfo.ModuleName) + ModuleController.STATUSBAR);
            if (stbview != null) region.Remove(stbview);

            RemoveItem(moduleInfo);
        }

        /// <summary>
        /// Adds new item to the Items collection of the default DocumentPane
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(ModuleLoadInfo item)
        {
            AddItem(item, DefaultDocumentPane);
        }

        /// <summary>
        /// Adds new item to the Items collection of the given DocumentPane
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        public void AddItem(ModuleLoadInfo item, DocumentPane target)
        {
            var ti = GetTabItem(item);
            target.Items.Add(ti);
        }

        /// <summary>
        /// Removes an item from the Items collection of the default DocumentPane
        /// </summary>
        /// <param name="item"></param>
        public void RemoveItem(ModuleLoadInfo item)
        {
            int tiInd = tabItems.IndexOf(s => Equals(s.Item1, item));
            if (tiInd == -1) throw new Exception("TabItem with the given ModuleLoadInfo doesn't exist!");
            DockableTabItem dti = tabItems[tiInd].Item2;
            Pane pane = dti.ContainerPane;
            pane.Items.Remove(dti);
            //if (pane != DefaultDocumentPane && pane.Items.Count == 0) pane.
            tabItems.RemoveAt(tiInd);
            if (pane.Items.Count == 0 && !Equals(pane, DefaultDocumentPane)) RemovePane(pane);
        }

        /// <summary>
        /// Gets the cached DockableTabItem instance of the given ModuleLoadInfo or if it doesn't exist creates a new one
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public DockableTabItem GetTabItem(ModuleLoadInfo info)
        {
            var ti = tabItems.FirstOrDefault(s => s.Item1.Equals(info));
            return (ti!=null ? ti.Item2 : CreateNewTab(info));
        }

        /// <summary>
        /// Reset DocumentPane's settings
        /// </summary>
        /// <param name="dummy"></param>
        public void ResetToDefaults(object dummy)
        {
            SetItems(Modules);
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<ModuleLoadInfo> source = value as List<ModuleLoadInfo>;
            if (source != null)
            {
                List<DocumentContent> result = new List<DocumentContent>();
                foreach (ModuleLoadInfo info in source)
                {
                    DockableTabItem ti = GetTabItem(info);
                    //ContentControl content = ti.Content as ContentControl;
                    result.Add(ti);
                }
                return result;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Closes a tab. This must be called to close a tab with a ModuleLoadInfo properly.
        /// </summary>
        public bool CloseTab(ModuleLoadInfo item, bool force)
        {
            ModuleViewModelBase viewModel = (ModuleViewModelBase)item.RenderedView.DataContext;
            if (!item.RenderedView.Close(force)) return false;
            eventAggregator.GetEvent<InactivateModuleContextEvent>().Publish(viewModel);
            viewModel.Dispose();
            RemoveModule(item);
            return true;
        }

        public void BringMLIToFront(ModuleLoadInfo mli)
        {
            DockableTabItem tabItem = (tabItems.FirstOrDefault(s => s.Item1==mli) ?? tabItems.First(s => s.Item1.Equals(mli))).Item2;
            tabItem.BringIntoView();
            tabItem.SetAsActive();

            UIThread.BeginRun(() =>
                                  {
                                      DocumentPane p = tabItem.ContainerPane as DocumentPane;
                                      if (p != null)
                                          p.BringToFront(tabItem);
                                  }, DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Temporary reference to the current Pane where a tab will be closed
        /// </summary>
        private Pane paneOfTheLastClosedTabItem;

        /// <summary>
        /// Occurs when a DockableTabItem is about to close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabItemClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DockableTabItem tabItem = sender as DockableTabItem;
            if (tabItem == null) return;
            paneOfTheLastClosedTabItem = tabItem.ContainerPane;
            if (!CloseTab(tabItem.LoadInfo, false))
            {
                e.Cancel = true;
                return;
            }
            tabItem.Closing -= OnTabItemClosing;
        }

        /// <summary>
        /// HACK: 'Cause AvalonDock fails to set the IsActiveContent property of the new "active" content we set it explicitly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabItemClosed(object sender, EventArgs e)
        {
            DockableTabItem tabItem = (DockableTabItem)sender;
            tabItem.Closed -= OnTabItemClosed;
            if (paneOfTheLastClosedTabItem.SelectedItem != null)
            {
                ((DockableTabItem) paneOfTheLastClosedTabItem.SelectedItem).SetAsActive();
                paneOfTheLastClosedTabItem = null;
            }
        }

        /// <summary>
        /// Obsolate
        /// </summary>
        /// <param name="tabItem"></param>
        [Obsolete]
        public void DetachDockableTabItem(DockableTabItem tabItem)
        {
            //ShellView view = ShellView.Current;
            //ShellViewModel viewModel = view.DataContext as ShellViewModel;
            //if (viewModel != null)
            //{
            //    view.FloatingTabs.Items.Add(tabItem);
            //    viewModel.DetachModule(tabItem.LoadInfo);
            //}
        }

        public void RegisterItem(DockableTabItem tabItem)
        {
            tabItems.Add(new Tuple<ModuleLoadInfo, DockableTabItem>(tabItem.LoadInfo, tabItem));
            tabItem.IsActiveContentChanged += OnTabItemIsActiveContentChanged;
            tabItem.Closing += OnTabItemClosing;
            tabItem.Closed += OnTabItemClosed;
        }

        private void OnTabItemIsActiveContentChanged(object sender, EventArgs e)
        {
            DockableTabItem tabItem = (DockableTabItem) sender;
            if (!tabItem.IsActiveContent) return;
            if (!tabItem.HasContent) moduleController.DisplayModule(tabItem.LoadInfo);
            moduleController.SetModuleContext(tabItem.LoadInfo);
        }

        #region IGeneralController Members

        public void Run()
        {
        }

        /// <summary>
        /// Resets the controller.
        /// </summary>
        public void Reset()
        {
            // close all tabs
            foreach (ModuleLoadInfo mli in tabItems.Select(s=>s.Item1).ToArray())
                CloseTab(mli, true);
            if (DefaultDocumentPane != null)
            {
                DefaultDocumentPane.Items.Clear();
                DefaultDocumentPane = null;
            }
            tabItems.Clear();
        }

        #endregion

    }
}
