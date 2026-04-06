# Subpath Hosting with Blazing.Mvvm

The `Blazing.SubpathHosting.Server` sample project demonstrates how to host a website using `Blazing.Mvvm` under a subpath of a web server. This is useful when you want to serve your application from a specific URL segment rather than the root of the domain.

## Configuration

1. To configure your Blazing.Mvvm application for subpath hosting, you need to add the `launchUrl` in the `launchSettings.json` file:

```json
"https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "fu/bar",
      "applicationUrl": "https://localhost:7037;http://localhost:5272",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
```

Here we set the `launchUrl` to `fu/bar`, which means the application will be accessible at `https://localhost:7037/fu/bar` or `http://localhost:5272/fu/bar`.

2. We need to configure the base path in the `Program.cs` file:

* First we need to configure Blazing.Mvvm:

```csharp
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.Server;
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
    options.BasePath = "/fu/bar/"; // Set the base path for the application
});
```

* Next we need to configure the ASP.NET Core middleware to use the same base path:

```csharp
app.UsePathBase("/fu/bar/"); // Set the base path for the application

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.Run();
```
* Lastly, we need to set the `<BasePath>` in the `index.html` (Web Assembly) or `_Host.cshtml` (Server) file:
```xml
<base href="/fu/bar/" />
```

## Running the Sample

1. Clone the repository.
2. Navigate to the `Blazing.SubpathHosting.Server` directory.
3. Run the application using `dotnet run`.
4. Open your browser and navigate to `http://localhost:5000/myapp`.
