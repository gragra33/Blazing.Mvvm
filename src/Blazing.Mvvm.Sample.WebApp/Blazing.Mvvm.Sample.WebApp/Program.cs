using Blazing.Mvvm;
using Blazing.Mvvm.Infrastructure;
using Blazing.Mvvm.Sample.WebApp.Client;
using Blazing.Mvvm.Sample.WebApp.Client.Pages;
using Blazing.Mvvm.Sample.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// register ViewModels
builder.Services.AddViewModels();

// enable MvvmNavigationManager
builder.Services.AddMvvmNavigation(options =>
{ 
    options.HostingModelType = BlazorHostingModelType.WebApp;
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
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();
