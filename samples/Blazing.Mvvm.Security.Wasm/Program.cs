using Blazing.Mvvm;
using Blazing.Mvvm.Sample.Shared.Data;
using Blazing.Mvvm.Sample.Shared.Services;
using Blazing.Mvvm.Sample.Shared.ViewModels;
using Blazing.Mvvm.Security.Wasm;
using Blazing.Mvvm.Security.Wasm.Data;
using Blazing.Mvvm.Security.Wasm.Providers;
using Blazing.Mvvm.Security.Wasm.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);

// Add application services
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IPostsService, PostsService>();

// Configure in-memory authentication with localStorage persistence
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<InMemoryAuthService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<LocalAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<LocalAuthStateProvider>());

// Add session lock service for monitoring authentication state
builder.Services.AddScoped<SessionLockService>();

// Add Blazing.Mvvm
builder.Services.AddMvvm(options =>
{
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;

    // Register Security.Wasm-specific ViewModels first so they take precedence
    options.RegisterViewModelsFromAssembly(typeof(App).Assembly);

    // Register Shared ViewModels (CounterViewModel, FetchDataViewModel, etc.)
    options.RegisterViewModelsFromAssembly(typeof(CounterViewModel).Assembly);
});

#if DEBUG
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

await builder.Build().RunAsync();
