using Blazing.Mvvm;
using Blazing.Mvvm.ParentChildSample;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// register ViewModels
builder.Services
    //.AddViewModels() // obsolete - now uses the ViewModelDefinition attribute & auto registration
    .AddMvvm(options =>
    {
        options.HostingModelType = BlazorHostingModelType.WebAssembly;
    });

await builder.Build().RunAsync();
