using Blazing.Mvvm;
using Blazing.Mvvm.Infrastructure;
using Blazing.Mvvm.Sample.Server;
using Blazing.Mvvm.Sample.Server.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// register ViewModels
builder.Services.AddViewModels();

// enable MvvmNavigationManager for server
builder.Services.AddMvvmNavigation(options =>
{ 
    options.HostingModelType = BlazorHostingModelType.Server;
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

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
