using Blazing.Mvvm;
using Blazing.Mvvm.Sample.Wasm;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);

// Add Blazing.Mvvm
builder.Services.AddMvvm();

#if DEBUG
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

await builder.Build().RunAsync();
