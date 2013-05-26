using System.Windows.Input;

namespace Neurotoxin.Contour.Presentation.Infrastructure
{
    public interface IDelegateCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }

}