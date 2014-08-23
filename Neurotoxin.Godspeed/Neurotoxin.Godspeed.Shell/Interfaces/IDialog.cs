using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IDialog<TViewModel, TPayload> : IView<TViewModel> where TViewModel : DialogViewModelBase<TPayload>
    {
    }
}