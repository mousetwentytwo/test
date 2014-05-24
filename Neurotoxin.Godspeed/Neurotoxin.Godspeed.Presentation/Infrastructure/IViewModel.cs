using System;
using System.ComponentModel;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public interface IViewModel : INotifyPropertyChanged, IDisposable
    {
        bool IsBusy { get; }
    }
}