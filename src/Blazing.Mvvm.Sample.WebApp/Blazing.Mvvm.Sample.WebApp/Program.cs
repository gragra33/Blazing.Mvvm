using Blazing.Mvvm;
using Blazing.Mvvm.Sample.WebApp.Client.Data;
using Blazing.Mvvm.Sample.WebApp.Components;
using Blazing.Mvvm.Sample.WebApp.Data;
using CommunityToolkit.Mvvm.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSingleton<IWeatherService, ServerWeatherService>();
builder.Services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);

// Add Blazing.Mvvm
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.WebApp;
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
    options.RegisterViewModelsFromAssemblyContaining<Blazing.Mvvm.Sample.WebApp.Client._Imports>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Blazing.Mvvm.Sample.WebApp.Client._Imports).Assembly);

app.MapGet("/api/weatherforecast", (IWeatherService weatherService, CancellationToken cancellationToken)
    => weatherService.GetForecastAsync(cancellationToken));

app.Run();
