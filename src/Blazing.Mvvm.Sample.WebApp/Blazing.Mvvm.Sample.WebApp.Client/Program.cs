using Blazing.Mvvm;
using Blazing.Mvvm.Infrastructure;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add Blazing.Mvvm
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.WebApp;
    options.RegisterViewModelsFromAssemblyContaining<Program>();
});

await builder.Build().RunAsync();
