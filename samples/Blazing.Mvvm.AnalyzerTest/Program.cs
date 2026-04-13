using Blazing.Mvvm.AnalyzerTest.Data;
using Blazing.Mvvm;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add services for testing BLAZMVVM0009
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddSingleton<HttpClient>();

// Add DbContext for BLAZMVVM0013 testing
builder.Services.AddDbContext<TestDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));

// Add Blazing.Mvvm
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.Server;
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<Blazing.Mvvm.AnalyzerTest.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
