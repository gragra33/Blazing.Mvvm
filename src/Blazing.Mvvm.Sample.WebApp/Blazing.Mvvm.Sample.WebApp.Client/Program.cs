using Blazing.Mvvm;
using Blazing.Mvvm.Sample.WebApp.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// register ViewModels
builder.Services.AddViewModels();

// enable MvvmNavigationManager
builder.Services.AddMvvmNavigation();

await builder.Build().RunAsync();
