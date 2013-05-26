using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;
using AvalonDock;
using Microsoft.Practices.Composite.Presentation.Regions;
using System.Reflection;
using System.Xml;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Neurotoxin.Contour.Presentation.Converters;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Presentation.Controls
{
    public class DockableTabItem : DocumentContent
    {
        private ContentControl contentControl;
        private bool isNew = true;

        public ModuleLoadInfo LoadInfo { get; set; }
        public ITabController Controller { get; set; }
        public ContentControl StatusBar { get; set; }

        public new bool HasContent
        {
            get
            {
                if (contentControl == null) return false;
                return contentControl.Content != null;
            }
        }

        protected override void OnClosed()
        {
            var template = (FrameworkElement) Content;
            var content = template.FindName("Content") as ContentControl;
            
            var regionManager = UnityInstance.Container.Resolve<IRegionManager>();
            
            string regionName = RegionManager.GetRegionName(content);
            //IRegion region = regionManager.Regions.First(s => s.Name == regionName);
            regionManager.Regions.Remove(regionName);
            RegionManager.SetRegionManager(content, null);

            regionName = RegionManager.GetRegionName(StatusBar);
            //region = regionManager.Regions.First(s => s.Name == regionName);
            regionManager.Regions.Remove(regionName);
            RegionManager.SetRegionManager(StatusBar, null);

            base.OnClosed();
        }

        public DockableTabItem()
        {
            this.LayoutUpdated += OnLayoutUpdated;
        }

        public DockableTabItem(ModuleLoadInfo info) : this()
        {
            Initialize(info, null);
        }

        public DockableTabItem(ModuleLoadInfo info, ITabController controller) : this()
        {
            Initialize(info, controller);
        }

        private void Initialize(ModuleLoadInfo info, ITabController controller)
        {
            var asm = Assembly.GetAssembly(typeof(ITabController));
            var templateStream = asm.GetManifestResourceStream("Neurotoxin.Contour.Presentation.Templates.TabContentTemplate.xaml");
            var template = System.Windows.Markup.XamlReader.Load(templateStream) as FrameworkElement;
            var content = template.FindName("Content") as ContentControl;
            Content = template;
            StatusBar = template.FindName("StatusBarContainer") as ContentControl;
            if (String.IsNullOrEmpty(info.Title)) info.Title = info.ModuleName;
            LoadInfo = info;
            IsFloatingAllowed = true;
            SetBinding(TitleProperty, new Binding("Title") { Source = info, Mode = BindingMode.OneWay });
            SetBinding(IconProperty, new Binding("RenderedView")
            {
                Source = info,
                Mode = BindingMode.OneWay,
                Converter = new ViewIconConverter()
            });
            var regionManager = UnityInstance.Container.Resolve<IRegionManager>();
            string regionName = info.GetUniqueRegionName(regionManager);
            RegionManager.SetRegionName(content, regionName);
            RegionManager.SetRegionManager(content, regionManager);
            RegionManager.SetRegionName(StatusBar, regionName + "StatusBar");
            RegionManager.SetRegionManager(StatusBar, regionManager);

            if (controller == null) return;
            Controller = controller;
            controller.RegisterItem(this);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (!isNew) return;
            DocumentPane dp = this.ContainerPane as DocumentPane;
            if (DocumentTabPanel.GetIsHeaderVisible(this))
            {
                isNew = false;
                return;
            }
            if (dp!=null)
                if (dp.Items[dp.Items.Count - 1] == this)
                    dp.BringToFront(this);
        }

        protected override void OnDragStart(Point ptMouse, Point ptRelativeMouse)
        {
            DockingManager dm = this.ContainerPane.GetManager();
            if (Controller != null) Controller.DetachDockableTabItem(this);
            base.OnDragStart(ptMouse, ptRelativeMouse);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            contentControl = base.Template.FindName("Content", this) as ContentControl;
            TextBlock title = base.Template.FindName("tabItemTitle", this) as TextBlock;
            if (title == null) return;
            title.MaxWidth = 200;
        }
    }
}