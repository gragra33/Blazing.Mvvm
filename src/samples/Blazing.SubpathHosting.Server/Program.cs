using Blazing.Mvvm;
using Blazing.SubpathHosting.Server.Data;
using CommunityToolkit.Mvvm.Messaging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IWeatherService, WeatherService>();
builder.Services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);

// Add Blazing.Mvvm
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.Server;
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
    options.BasePath = "/fu/bar/"; // Set the base path for the application
});

#if DEBUG
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UsePathBase("/fu/bar/");
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub("/_blazor"); 
app.MapFallbackToPage("/_Host");

await app.RunAsync();