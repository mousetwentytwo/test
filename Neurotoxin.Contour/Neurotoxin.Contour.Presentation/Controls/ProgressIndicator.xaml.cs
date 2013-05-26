using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Timers;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Presentation.Controls
{
    /// <summary>
    /// Common control for progress indicating purposes with an SQL Management Studio-ish circular bar. 
    /// </summary>
    public partial class ProgressIndicator : UserControl
    {
        private Timer _sleep;

        public ProgressIndicator()
        {
            InitializeComponent();
            _sleep = new Timer(100);
            _sleep.Elapsed += _sleep_Elapsed;
            this.Loaded += ProgressIndicator_Loaded;
        }

        void _sleep_Elapsed(object sender, ElapsedEventArgs e)
        {
            UIThread.BeginRun(Step);
        }

        void ProgressIndicator_Loaded(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible) Start();
        }

        public void Stop()
        {
            _sleep.Stop();
        }

        public void Start()
        {
            Restore();
            _sleep.Start();
        }

        private void Step()
        {
            foreach (UIElement o in Content.Children)
            {
                RotateTransform rt = (RotateTransform)o.RenderTransform;
                rt.Angle += 30;
                if (rt.Angle == 360) rt.Angle = 0;
            }
            _sleep.Start();
        }

        private void Restore()
        {
            int i = 0;
            foreach (UIElement o in Content.Children)
            {
                ((RotateTransform)o.RenderTransform).Angle = i;
                i += 30;
            }
        }

       protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == VisibilityProperty)
            {
                if ((Visibility)e.NewValue == Visibility.Visible) Start();
                if ((Visibility)e.NewValue == Visibility.Collapsed) Stop();
            }
        }
    }
}