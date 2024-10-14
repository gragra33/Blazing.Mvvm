using Blazing.Mvvm;
using Blazing.Mvvm.Sample.WebApp.Client.Data;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IWeatherService, ClientWeatherService>();
builder.Services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);

// Add Blazing.Mvvm
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.WebApp;
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
});

await builder.Build().RunAsync();
