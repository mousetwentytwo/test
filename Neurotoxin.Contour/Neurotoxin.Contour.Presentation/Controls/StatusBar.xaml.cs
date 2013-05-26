using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Primitives;

namespace Neurotoxin.Contour.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for StatusBar.xaml
    /// </summary>
    public partial class StatusBar : StatusBarBase
    {
        public StatusBar()
        {
            InitializeComponent();
        }

        public StatusBar(ModuleViewModelBase viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}