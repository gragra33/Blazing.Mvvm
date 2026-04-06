using System.ComponentModel;
using System.Text;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Sample.WebApp.Client.Data;
using Blazing.Mvvm.Sample.WebApp.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Web;

namespace Blazing.Mvvm.Sample.WebApp.Client.ViewModels;

// These test-local fixtures intentionally mirror the original sample namespaces so existing src tests can stay focused while remaining independent from /samples.

[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
public sealed partial class CounterViewModel : ViewModelBase
{
    private readonly ILogger<CounterViewModel> _logger;

    [ObservableProperty]
    private int _currentCount;

    public CounterViewModel(ILogger<CounterViewModel> logger)
        => _logger = logger;

    public void IncrementCount()
        => CurrentCount++;

    public void ResetCount()
        => CurrentCount = 0;

    public override void OnInitialized() => LogLifeCycleEvent(nameof(OnInitialized));
    public override Task OnInitializedAsync()
    {
        LogLifeCycleEvent(nameof(OnInitializedAsync));
        return base.OnInitializedAsync();
    }
    public override void OnParametersSet() => LogLifeCycleEvent(nameof(OnParametersSet));
    public override Task OnParametersSetAsync() { LogLifeCycleEvent(nameof(OnParametersSetAsync)); return Task.CompletedTask; }
    public override void OnAfterRender(bool firstRender) => LogLifeCycleEvent(nameof(OnAfterRender));
    public override Task OnAfterRenderAsync(bool firstRender) { LogLifeCycleEvent(nameof(OnAfterRenderAsync)); return Task.CompletedTask; }
    public override bool ShouldRender() { LogLifeCycleEvent(nameof(ShouldRender)); return true; }

    private void LogLifeCycleEvent(string method)
        => _logger.LogInformation("{ViewModel} => Life-cycle event: {LifeCycleMethod}.", nameof(CounterViewModel), method);
}

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class FetchDataViewModel : ViewModelBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<FetchDataViewModel> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [ObservableProperty]
    private IEnumerable<WeatherForecast>? _weatherForecasts;

    public FetchDataViewModel(IWeatherService weatherService, ILogger<FetchDataViewModel> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    public Task PersistStateAsync(PersistentComponentState state)
    {
        state.PersistAsJson(nameof(WeatherForecasts), WeatherForecasts);
        return Task.CompletedTask;
    }

    public async Task LoadStateAsync(PersistentComponentState state)
    {
        if (state.TryTakeFromJson<IEnumerable<WeatherForecast>>(nameof(WeatherForecasts), out var weatherForecasts))
        {
            WeatherForecasts = weatherForecasts!;
            return;
        }

        WeatherForecasts = await _weatherService.GetForecastAsync(_cancellationTokenSource.Token) ?? [];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logger.LogInformation("Disposing FetchDataViewModel.");
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        base.Dispose(disposing);
    }
}

[ViewModelDefinition]
public sealed partial class EditContactViewModel : ViewModelBase
{
    private readonly ILogger<EditContactViewModel> _logger;

    [ObservableProperty]
    private ContactInfo _contact = new();

    public EditContactViewModel(ILogger<EditContactViewModel> logger)
    {
        _logger = logger;
        Contact.PropertyChanged += ContactOnPropertyChanged;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Contact.PropertyChanged -= ContactOnPropertyChanged;
        }

        base.Dispose(disposing);
    }

    [RelayCommand]
    private void ClearForm()
        => Contact = new ContactInfo();

    [RelayCommand]
    private void Save()
        => _logger.LogInformation("Form is valid and submitted!");

    private void ContactOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        => NotifyStateChanged();
}

[ViewModelDefinition(Key = nameof(HexTranslateViewModel))]
public sealed class HexTranslateViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;

    public HexTranslateViewModel(IMessenger messenger)
        => _messenger = messenger;

    public void ResetChildInputs()
        => _messenger.Send(new ResetHexAsciiInputsMessage());
}

[ViewModelDefinition]
public sealed partial class HexEntryViewModel : RecipientViewModelBase<ConvertAsciiToHexMessage>, IRecipient<ResetHexAsciiInputsMessage>
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendToAsciiConverterCommand))]
    private string? _hexText;

    public override void Receive(ConvertAsciiToHexMessage message)
    {
        StringBuilder builder = new();
        foreach (char c in message.AsciiToConvert)
        {
            builder.AppendFormat("{0:X}", Convert.ToInt32(c));
        }

        HexText = builder.ToString();
    }

    public void Receive(ResetHexAsciiInputsMessage message)
        => HexText = string.Empty;

    [RelayCommand(CanExecute = nameof(CanSendToAsciiConverter))]
    private void SendToAsciiConverter()
        => Messenger.Send(new ConvertHexToAsciiMessage(HexText!));

    private bool CanSendToAsciiConverter()
        => !string.IsNullOrWhiteSpace(HexText);
}

[ViewModelDefinition]
public sealed partial class TextEntryViewModel : RecipientViewModelBase<ConvertHexToAsciiMessage>, IRecipient<ResetHexAsciiInputsMessage>
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendToHexConverterCommand))]
    private string? _asciiText;

    public override void Receive(ConvertHexToAsciiMessage message)
    {
        StringBuilder builder = new();

        for (int i = 0; i < message.HexToConvert.Length; i += 2)
        {
            string hex = message.HexToConvert.Substring(i, 2);
            builder.Append(Convert.ToChar(Convert.ToUInt32(hex, 16)));
        }

        AsciiText = builder.ToString();
    }

    public void Receive(ResetHexAsciiInputsMessage message)
        => AsciiText = string.Empty;

    [RelayCommand(CanExecute = nameof(CanSendToHexConverter))]
    private void SendToHexConverter()
        => Messenger.Send(new ConvertAsciiToHexMessage(AsciiText!));

    private bool CanSendToHexConverter()
        => !string.IsNullOrWhiteSpace(AsciiText);
}

[ViewModelDefinition]
public sealed partial class MainLayoutViewModel : ViewModelBase
{
    private readonly NavigationManager _navigationManager;

    [ObservableProperty]
    private int _counter;

    public MainLayoutViewModel(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _navigationManager.LocationChanged -= OnLocationChanged;
        }

        base.Dispose(disposing);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => Counter++;
}

public interface ITestNavigationViewModel : IViewModelBase, IDisposable
{
    string QueryString { get; set; }
    string Test { get; set; }
    string? Echo { get; set; }
    RelayCommand HexTranslateNavigateCommand { get; }
    RelayCommand<string> TestNavigateCommand { get; }
}

public interface ITestKeyedNavigationViewModel : IViewModelBase
{
    RelayCommand<string> TestNavigateCommand { get; }

    /// <inheritdoc cref="TestNavigationBaseViewModel._queryString"/>
    string? QueryString { get; set; }

    /// <inheritdoc cref="TestNavigationBaseViewModel._test"/>
    string? Test { get; set; }

    // populated by MvvmComponentBase
    string? Echo { get; set; }

    RelayCommand HexTranslateNavigateCommand { get; }
}

public abstract partial class TestNavigationBaseViewModel : ViewModelBase, ITestNavigationViewModel
{
    internal readonly IMvvmNavigationManager MvvmNavigationManager;
    internal readonly NavigationManager NavigationManager;

    private RelayCommand? _hexTranslateNavigateCommand;
    private bool _isDisposed;
    internal RelayCommand<string>? TestNavigateCommandImpl;

    [ObservableProperty]
    private string? _queryString;

    [ObservableProperty]
    private string? _test;

    protected TestNavigationBaseViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    {
        MvvmNavigationManager = mvvmNavigationManager;
        NavigationManager = navigationManager;
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    [ViewParameter]
    public string? Echo { get; set; } = string.Empty;

    public RelayCommand HexTranslateNavigateCommand
        => _hexTranslateNavigateCommand ??= new RelayCommand(() => Navigate<HexTranslateViewModel>());

    public virtual RelayCommand<string> TestNavigateCommand
        => TestNavigateCommandImpl ??= new RelayCommand<string>(Navigate<ITestNavigationViewModel>);

    public override void OnInitialized()
        => ProcessQueryString();

    protected override void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }

        _isDisposed = true;
        base.Dispose(disposing);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => ProcessQueryString();

    private void Navigate<T>(string? @params = null)
        where T : IViewModelBase
    {
        if (string.IsNullOrWhiteSpace(@params))
        {
            MvvmNavigationManager.NavigateTo<T>();
            return;
        }

        MvvmNavigationManager.NavigateTo<T>(@params);
    }

    private void ProcessQueryString()
    {
        QueryString = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query;
        Test = HttpUtility.ParseQueryString(QueryString ?? string.Empty)["test"] ?? string.Empty;
    }
}

[ViewModelDefinition<ITestNavigationViewModel>]
public sealed class TestNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    : TestNavigationBaseViewModel(mvvmNavigationManager, navigationManager);

[ViewModelDefinition<ITestKeyedNavigationViewModel>(Key = nameof(TestKeyedNavigationViewModel))]
public sealed class TestKeyedNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    : TestNavigationBaseViewModel(mvvmNavigationManager, navigationManager), ITestKeyedNavigationViewModel
{
    public override RelayCommand<string> TestNavigateCommand
        => TestNavigateCommandImpl ??= new RelayCommand<string>(value => Navigate(nameof(TestKeyedNavigationViewModel), value));

    private void Navigate(string key, string? @params = null)
    {
        if (string.IsNullOrWhiteSpace(@params))
        {
            MvvmNavigationManager.NavigateTo(key);
            return;
        }

        MvvmNavigationManager.NavigateTo(key, @params);
    }
}
