using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

public record TestMessage(string Data);

// BLAZMVVM0015 & BLAZMVVM0016: Missing Dispose(bool disposing) override for messenger registration/resource cleanup
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test015And016ViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient;

    public Test015And016ViewModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _ = _httpClient.BaseAddress;
        
        // BLAZMVVM0016: Messenger registration without unregistration
        WeakReferenceMessenger.Default.Register<TestMessage>(this, HandleMessage);
    }

    [ObservableProperty]
    private string _message = string.Empty;

    private void HandleMessage(object recipient, TestMessage message)
    {
        Message = message.Data;
    }

    // Missing Dispose(bool disposing) override - triggers BLAZMVVM0015 and BLAZMVVM0016
    // Should dispose _httpClient and unregister messenger, then call base.Dispose(disposing)
}
