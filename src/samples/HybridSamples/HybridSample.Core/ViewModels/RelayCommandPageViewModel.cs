using System.Windows.Input;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// ViewModel for demonstrating usage of <see cref="RelayCommand"/> in the sample app.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class RelayCommandPageViewModel : SamplePageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandPageViewModel"/> class.
    /// </summary>
    /// <param name="filesService">The service for file operations.</param>
    public RelayCommandPageViewModel(IFilesService filesService) 
        : base(filesService)
    {
        IncrementCounterCommand = new RelayCommand(IncrementCounter);
    }

    /// <summary>
    /// Gets the <see cref="ICommand"/> responsible for incrementing <see cref="Counter"/>.
    /// </summary>
    public ICommand IncrementCounterCommand { get; }

    private int counter;

    /// <summary>
    /// Gets the current value of the counter.
    /// </summary>
    public int Counter
    {
        get => counter;
        private set => SetProperty(ref counter, value);
    }

    /// <summary>
    /// Increments <see cref="Counter"/>.
    /// </summary>
    private void IncrementCounter() => Counter++;

}
