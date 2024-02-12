using Blazing.Mvvm;
using Blazing.Mvvm.Sample.Wasm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Blazing.Mvvm
builder.Services.AddMvvm();

// Auto discovery is the default behaviour.
// However, you can specify the assemblies to scan for view models, which is useful when you want to scan only specific assemblies and also helps to reduce the startup time.
//builder.Services.AddMvvm(opt => opt.RegisterViewModelsFromAssemblyContaining<Program>());

#if DEBUG
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

await builder.Build().RunAsync();
