using System.Windows.Input;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// ViewModel for demonstrating usage of <see cref="ObservableObject"/> in the sample app.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class ObservableObjectPageViewModel : SamplePageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableObjectPageViewModel"/> class.
    /// </summary>
    /// <param _name="filesService">The service for file operations.</param>
    public ObservableObjectPageViewModel(IFilesService filesService) 
        : base(filesService)
    {
        ReloadTaskCommand = new RelayCommand(ReloadTask);
    }

    /// <summary>
    /// Gets the <see cref="ICommand"/> responsible for setting <see cref="MyTask"/>.
    /// </summary>
    public ICommand ReloadTaskCommand { get; }

    private string? _name;

    /// <summary>
    /// Gets or sets the _name to display.
    /// </summary>
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private TaskNotifier? _myTask;

    /// <summary>
    /// Gets or sets the asynchronous task notifier.
    /// </summary>
    public Task? MyTask
    {
        get => _myTask;
        private set => SetPropertyAndNotifyOnCompletion(ref _myTask, value);
    }

    /// <summary>
    /// Simulates an asynchronous method.
    /// </summary>
    public void ReloadTask()
    {
        MyTask = Task.Delay(3000);
    }
}
