using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

public record TestMessage(string Data);

// BLAZMVVM0015 & BLAZMVVM0016: Missing IDisposable for messenger registration
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test015And016ViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient;

    public Test015And016ViewModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // BLAZMVVM0016: Messenger registration without unregistration
        WeakReferenceMessenger.Default.Register<TestMessage>(this, HandleMessage);
    }

    [ObservableProperty]
    private string _message = string.Empty;

    private void HandleMessage(object recipient, TestMessage message)
    {
        Message = message.Data;
    }

    // Missing IDisposable implementation - triggers BLAZMVVM0015
    // Should dispose _httpClient and unregister messenger
}
