using Blazing.Mvvm;
using Blazing.Mvvm.Infrastructure;
using Blazing.Mvvm.Sample.Server.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Add Blazing.Mvvm
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.Server;

    // Auto discovery is the default behaviour.
    // However, you can specify the assemblies to scan for view models, which is useful when you want to scan only specific assemblies and also helps to reduce the startup time.
    //options.RegisterViewModelsFromAssemblyContaining<Program>();
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
