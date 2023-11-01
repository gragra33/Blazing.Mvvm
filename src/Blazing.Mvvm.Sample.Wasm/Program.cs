using Blazing.Mvvm;
using Blazing.Mvvm.Sample.Wasm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// register ViewModels
builder.Services.AddViewModels();

// enable MvvmNavigationManager
builder.Services.AddMvvmNavigation();

#if DEBUG
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

await builder.Build().RunAsync();
