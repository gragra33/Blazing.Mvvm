using Blazing.Mvvm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ParameterResolution.Sample.Wasm;

// Build the WebAssembly host with default configuration
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register root components for the Blazor application
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient as a scoped service with the application's base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Blazing.Mvvm services with Parameter Resolution enabled
// This configures the MVVM framework for WebAssembly hosting and enables parameter resolution
// between Views and ViewModels, allowing automatic mapping of component parameters to ViewModel properties
builder.Services.AddMvvm(options =>
{
    // Set the hosting model type to WebAssembly for proper service lifetime management
    options.HostingModelType = BlazorHostingModelType.WebAssembly;
    
    // Enable parameter resolution in both View and ViewModel
    // This allows properties marked with [ViewParameter] in ViewModels to be automatically
    // populated from [Parameter] properties defined in the corresponding View components
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
});

// Build and run the application
await builder.Build().RunAsync();
