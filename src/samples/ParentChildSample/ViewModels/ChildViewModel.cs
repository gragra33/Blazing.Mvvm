using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.ParentChildSample.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.ParentChildSample.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class ChildViewModel : ViewModelBase
{
    [ObservableProperty]
    public string _text = "Child";

    [RelayCommand]
    public virtual void Close()
        // tell the parent to close us...
        => WeakReferenceMessenger.Default.Send(new ChildMessage(Text));
}