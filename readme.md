# Blazor Extension for the MVVM CommunityToolkit

🔥 **Blazing.Mvvm** brings full MVVM support to Blazor applications through seamless integration with the [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/). This library supports all Blazor hosting models including Server, WebAssembly (WASM), Static Server-Side Rendering (SSR), Auto, Hybrid (WPF, WinForms, Avalonia), and MAUI. It features strongly-typed ViewModel-first navigation, automatic ViewModel registration and discovery, parameter resolution between Views and ViewModels, validation support with `ObservableValidator`, and comprehensive lifecycle management. The library includes extensive sample projects and complete documentation to help you get started quickly.

## Table of Contents

<!-- TOC -->
- [Blazor Extension for the MVVM CommunityToolkit](#blazor-extension-for-the-mvvm-communitytoolkit)
  - [Table of Contents](#table-of-contents)
  - [Quick Start](#quick-start)
    - [Installation](#installation)
      - [.NET CLI](#net-cli)
      - [NuGet Package Manager](#nuget-package-manager)
    - [Configuration](#configuration)
      - [Registering ViewModels in a Different Assembly](#registering-viewmodels-in-a-different-assembly)
    - [Usage](#usage)
      - [Create a `ViewModel` inheriting the `ViewModelBase` class](#create-a-viewmodel-inheriting-the-viewmodelbase-class)
      - [Create your Page inheriting the `MvvmComponentBase<TViewModel>` component](#create-your-page-inheriting-the-mvvmcomponentbasetviewmodel-component)
  - [Give a ⭐](#give-a-)
  - [Documentation](#documentation)
    - [View Model](#view-model)
      - [Lifecycle Methods](#lifecycle-methods)
      - [Service Registration](#service-registration)
        - [Registering ViewModels with Interfaces or Abstract Classes](#registering-viewmodels-with-interfaces-or-abstract-classes)
        - [Registering Keyed ViewModels](#registering-keyed-viewmodels)
      - [Parameter Resolution](#parameter-resolution)
    - [MVVM Navigation](#mvvm-navigation)
      - [Navigate by abstraction](#navigate-by-abstraction)
    - [MVVM Validation](#mvvm-validation)
    - [Subpath Hosting](#subpath-hosting)
    - [Complex Multi-Project ViewModel Registration](#complex-multi-project-viewmodel-registration)
    - [Sample Projects](#sample-projects)
      - [Running Samples with Different .NET Target Frameworks](#running-samples-with-different-net-target-frameworks)
  - [History](#history)
    - [V3.0.0](#v300)
    - [V2.0.0](#v200)
<!-- TOC -->

## Quick Start

### Installation

Add the [Blazing.Mvvm](https://www.nuget.org/packages/Blazing.Mvvm) NuGet package to your project.

Install the package via .NET CLI or the NuGet Package Manager.

#### .NET CLI

```bash
dotnet add package Blazing.Mvvm
```

#### NuGet Package Manager

```powershell
Install-Package Blazing.Mvvm
```

### Configuration

Configure the library in your `Program.cs` file. The `AddMvvm` method will add the required services for the library and automatically register ViewModels that inherit from the `ViewModelBase`, `RecipientViewModelBase`, or `ValidatorViewModelBase` class in the calling assembly.

```csharp
using Blazing.Mvvm;

builder.Services.AddMvvm(options =>
{ 
    options.HostingModelType = BlazorHostingModelType.WebApp;
});
```

If you are using a different hosting model, set the `HostingModelType` property to the appropriate value. The available options are:

- `BlazorHostingModelType.Hybrid`
- `BlazorHostingModelType.Server`
- `BlazorHostingModelType.WebApp`
- `BlazorHostingModelType.WebAssembly`
- `BlazorHostingModelType.HybridMaui`

#### Registering ViewModels in a Different Assembly

If the ViewModels are in a different assembly, configure the library to scan that assembly for the ViewModels.

```csharp
using Blazing.Mvvm;

builder.Services.AddMvvm(options =>
{ 
    options.RegisterViewModelsFromAssemblyContaining<MyViewModel>();
});

// OR

var vmAssembly = typeof(MyViewModel).Assembly;
builder.Services.AddMvvm(options =>
{ 
    options.RegisterViewModelsFromAssembly(vmAssembly);
});
```

### Usage

#### Create a `ViewModel` inheriting the `ViewModelBase` class

```csharp
public partial class FetchDataViewModel : ViewModelBase
{
    private static readonly string[] Summaries = [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [ObservableProperty]
    private ObservableCollection<WeatherForecast> _weatherForecasts = new();

    public string Title => "Weather forecast";

    public override void OnInitialized()
        => WeatherForecasts = new ObservableCollection<WeatherForecast>(Get());

    private IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        });
    }
}
```

#### Create your Page inheriting the `MvvmComponentBase<TViewModel>` component

> ***NOTE:*** If working with repositories, database services, etc, that require a scope, then use `MvvmOwningComponentBase<TViewModel>` instead.

```xml
@page "/fetchdata"
@inherits MvvmOwningComponentBase<FetchDataViewModel>

<PageTitle>@ViewModel.Title</PageTitle>

<h1>@ViewModel.Title</h1>

@if (!ViewModel.WeatherForecasts.Any())
{
    <p><em>Loading...</em></p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Date</th>
                <th>Temp. (C)</th>
                <th>Temp. (F)</th>
                <th>Summary</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var forecast in ViewModel.WeatherForecasts)
            {
                <tr>
                    <td>@forecast.Date.ToShortDateString()</td>
                    <td>@forecast.TemperatureC</td>
                    <td>@forecast.TemperatureF</td>
                    <td>@forecast.Summary</td>
                </tr>
            }
        </tbody>
    </table>
}
```

## Give a ⭐

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

Also, if you find this library useful, and you're feeling really generous, then please consider [buying me a coffee ☕](https://bmc.link/gragra33).

## Documentation

The Library supports the following hosting models:

- Blazor Server App
- Blazor WebAssembly App (WASM)
- Blazor Web App (.NET 8.0+)
- Blazor Hybrid - Wpf, WinForms, MAUI, and Avalonia (Windows only)

The library package includes:

- `MvvmComponentBase`, `MvvmOwningComponentBase` (Scoped service support), & `MvvmLayoutComponentBase` for quick and easy wiring up ViewModels.
- `ViewModelBase`, `RecipientViewModelBase`, & `ValidatorViewModelBase` wrappers for the [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/).
- `MvvmNavigationManager` class, `MvvmNavLink`, and `MvvmKeyNavLink` component for MVVM-style navigation, no more hard-coded paths.
- Sample applications for getting started quickly with all hosting models.

### View Model

The library offers several base classes that extend the [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) base classes:

- `ViewModelBase`: Inherits from the [`ObservableObject`](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/observableobject) class.
- `RecipientViewModelBase`: Inherits from the [`ObservableRecipient`](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/observablerecipient) class.
- `ValidatorViewModelBase`: Inherits from the [`ObservableValidator`](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/observablevalidator) class and supports the `EditForm` component.

#### Lifecycle Methods

The `ViewModelBase`, `RecipientViewModelBase`, and `ValidatorViewModelBase` classes support the `ComponentBase` lifecycle methods, which are invoked when the corresponding `ComponentBase` method is called:

- `OnAfterRender`
- `OnAfterRenderAsync`
- `OnInitialized`
- `OnInitializedAsync`
- `OnParametersSet`
- `OnParametersSetAsync`
- `ShouldRender`

#### Service Registration

ViewModels are registered as `Transient` services by default. If you need to register a ViewModel with a different service lifetime (Scoped, Singleton, Transient), use the `ViewModelDefinition` attribute:

```csharp
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public partial class FetchDataViewModel : ViewModelBase
{
    // ViewModel code
}
```

In the `View` component, inherit the `MvvmComponentBase` type and set the generic argument to the `ViewModel`:

```xml
@page "/fetchdata"
@inherits MvvmComponentBase<FetchDataViewModel>
```

##### Registering ViewModels with Interfaces or Abstract Classes

To register the `ViewModel` with a specific interface or abstract class, use the `ViewModelDefinition` generic attribute:

```csharp
[ViewModelDefinition<IFetchDataViewModel>]
public partial class FetchDataViewModel : ViewModelBase, IFetchDataViewModel
{
    // ViewModel code
}
```

In the `View` component, inherit the `MvvmComponentBase` type and set the generic argument to the interface or abstract class:

```xml
@page "/fetchdata"
@inherits MvvmComponentBase<IFetchDataViewModel>
```

##### Registering Keyed ViewModels

To register the `ViewModel` as a keyed service, use the `ViewModelDefinition` attribute (this also applies to generic variant) and set the `Key` property:

```csharp
[ViewModelDefinition(Key = "FetchDataViewModel")]
public partial class FetchDataViewModel : ViewModelBase
{
    // ViewModel code
}
```

In the `View` component, use the `ViewModelKey` attribute to specify the key of the `ViewModel`:

```xml
@page "/fetchdata"
@attribute [ViewModelKey("FetchDataViewModel")]
@inherits MvvmComponentBase<FetchDataViewModel>
```

#### Parameter Resolution

The library supports passing parameter values to the `ViewModel` which are defined in the `View`.

This feature is opt-in. To enable it, set the `ParameterResolutionMode` property to `ViewAndViewModel` in the `AddMvvm` method. This will resolve parameters in both the `View` component and the `ViewModel`.

```csharp
builder.Services.AddMvvm(options =>
{ 
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
});
```

To resolve parameters in the `ViewModel` only, set the `ParameterResolutionMode` property value to `ViewModel`.

Properties in the `ViewModel` that should be set must be marked with the `ViewParameter` attribute.

```csharp
public partial class SampleViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private string _title;

    [ViewParameter]
    public int Count { get; set; }

    [ViewParameter("Content")]
    private string Body { get; set; }
}
```

In the `View` component, the parameters should be defined as properties with the `Parameter` attribute:

```xml
@inherits MvvmComponentBase<SampleViewModel>

@code {
    [Parameter]
    public string Title { get; set; }

    [Parameter]
    public int Count { get; set; }

    [Parameter]
    public string Content { get; set; }
}
```

### MVVM Navigation

No more magic strings! Strongly-typed navigation is now possible. If the page URI changes, you no longer need to search through your source code to make updates. It is auto-magically resolved at runtime for you!

When the `MvvmNavigationManager` is initialized by the IOC container as a Singleton, the class examines all assemblies and internally caches all ViewModels (classes and interfaces) along with their associated pages.

When navigation is required, a quick lookup is performed, and the Blazor `NavigationManager` is used to navigate to the correct page. Any relative URI or query string passed via the `NavigateTo` method call is also included.

> **Note:** The `MvvmNavigationManager` class is not a complete replacement for the Blazor `NavigationManager` class; it only adds support for MVVM.

**Modify the `NavMenu.razor` to use `MvvmNavLink`:**

```xml
<div class="nav-item px-3">
    <MvvmNavLink class="nav-link" TViewModel="FetchDataViewModel">
        <span class="oi oi-list-rich" aria-hidden="true"></span> Fetch data
    </MvvmNavLink>
</div>
```

> The `MvvmNavLink` component is based on the Blazor `NavLink` component and includes additional `TViewModel` and `RelativeUri` properties. Internally, it uses the `MvvmNavigationManager` for navigation.

**Navigate by ViewModel using the `MvvmNavigationManager` from code:**

Inject the `MvvmNavigationManager` class into your page or ViewModel, then use the `NavigateTo` method:

```csharp
mvvmNavigationManager.NavigateTo<FetchDataViewModel>();
```

The `NavigateTo` method works the same as the standard Blazor `NavigationManager` and also supports passing a relative URL and/or query string.

#### Navigate by abstraction

If you prefer abstraction, you can also navigate by interface as shown below:

```csharp
mvvmNavigationManager.NavigateTo<ITestNavigationViewModel>();
```

The same principle works with the `MvvmNavLink` component:

```xml
<div class="nav-item px-3">
    <MvvmNavLink class="nav-link"
                 TViewModel=ITestNavigationViewModel
                 Match="NavLinkMatch.All">
        <span class="oi oi-calculator" aria-hidden="true"></span>Test
    </MvvmNavLink>
</div>
<div class="nav-item px-3">
    <MvvmNavLink class="nav-link"
                 TViewModel=ITestNavigationViewModel
                 RelativeUri="this is a MvvmNavLink test"
                 Match="NavLinkMatch.All">
        <span class="oi oi-calculator" aria-hidden="true"></span>Test + Params
    </MvvmNavLink>
</div>
<div class="nav-item px-3">
    <MvvmNavLink class="nav-link"
                 TViewModel=ITestNavigationViewModel
                 RelativeUri="?test=this%20is%20a%20MvvmNavLink%20querystring%20test"
                 Match="NavLinkMatch.All">
        <span class="oi oi-calculator" aria-hidden="true"></span>Test + QueryString
    </MvvmNavLink>
</div>
<div class="nav-item px-3">
    <MvvmNavLink class="nav-link"
                 TViewModel=ITestNavigationViewModel
                 RelativeUri="this is a MvvmNvLink test/?test=this%20is%20a%20MvvmNavLink%20querystring%20test"
                 Match="NavLinkMatch.All">
        <span class="oi oi-calculator" aria-hidden="true"></span>Test + Both
    </MvvmNavLink>
</div>
```

**Navigate by ViewModel Key using the `MvvmNavigationManager` from code:**

Inject the `MvvmNavigationManager` class into your page or ViewModel, then use the `NavigateTo` method:

```csharp
MvvmNavigationManager.NavigateTo("FetchDataViewModel");
```

The same principle works with the `MvvmKeyNavLink` component:

```xml
<div class="nav-item px-3">
    <MvvmKeyNavLink class="nav-link"
                    NavigationKey="@nameof(TestKeyedNavigationViewModel)"
                    Match="NavLinkMatch.All">
        <span class="oi oi-calculator" aria-hidden="true"></span> Keyed Test
    </MvvmKeyNavLink>
</div>

<div class="nav-item px-3">
    <MvvmKeyNavLink class="nav-link"
                    NavigationKey="@nameof(TestKeyedNavigationViewModel)"
                    RelativeUri="this is a MvvmKeyNavLink test"
                    Match="NavLinkMatch.All">
        <span class="oi oi-calculator" aria-hidden="true"></span> Keyed + Params
    </MvvmKeyNavLink>
</div>

<div class="nav-item px-3">
    <MvvmKeyNavLink class="nav-link"
                    NavigationKey="@nameof(TestKeyedNavigationViewModel)"
                    RelativeUri="?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test"
                    Match="NavLinkMatch.All">
        <span class="oi oi-calculator" aria-hidden="true"></span> Keyed + QueryString
    </MvvmKeyNavLink>
</div>

<div class="nav-item px-3">
    <MvvmKeyNavLink class="nav-link"
                    NavigationKey="@nameof(TestKeyedNavigationViewModel)"
                    RelativeUri="this is a MvvmKeyNavLink test/?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test"
                    Match="NavLinkMatch.All">
        <span class="oi oi-calculator" aria-hidden="true"></span> Keyed + Both
    </MvvmKeyNavLink>
</div>
```

### MVVM Validation

The library provides an `MvvmObservableValidator` component that works with the `EditForm` component to enable validation using the `ObservableValidator` class from the [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) library.

The following example demonstrates how to use the `MvvmObservableValidator` component with the `EditForm` component to perform validation.

**First, define a class that inherits from the `ObservableValidator` class and contains properties with validation attributes:**

```csharp
public class ContactInfo : ObservableValidator
{
    private string? _name;

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "The {0} field must have a length between {2} and {1}.")]
    [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "The {0} field contains invalid characters. Only letters, spaces, apostrophes, and hyphens are allowed.")]
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value, true);
    }

    private string? _email;

    [Required]
    [EmailAddress]
    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value, true);
    }

    private string? _phoneNumber;

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value, true);
    }
}
```

**Next, in the `ViewModel` component, define the property that will hold the object to be validated and the methods that will be called when the form is submitted:**

```csharp
public sealed partial class EditContactViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<EditContactViewModel> _logger;

    [ObservableProperty]
    private ContactInfo _contact = new();

    public EditContactViewModel(ILogger<EditContactViewModel> logger)
    {
        _logger = logger;
        Contact.PropertyChanged += ContactOnPropertyChanged;
    }

    public void Dispose()
        => Contact.PropertyChanged -= ContactOnPropertyChanged;

    [RelayCommand]
    private void ClearForm()
        => Contact = new ContactInfo();

    [RelayCommand]
    private void Save()
        => _logger.LogInformation("Form is valid and submitted!");

    private void ContactOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        => NotifyStateChanged();
}
```

**Finally, in the `View` component, use the `EditForm` component with the `MvvmObservableValidator` component to enable validation:**

```xml
@page "/form"
@inherits MvvmComponentBase<EditContactViewModel>

<EditForm Model="ViewModel.Contact" FormName="EditContact" OnValidSubmit="ViewModel.SaveCommand.Execute">
    <MvvmObservableValidator />
    <ValidationSummary />

    <div class="row g-3">
        <div class="col-12">
            <label class="form-label">Name:</label>
            <InputText aria-label="name" @bind-Value="ViewModel.Contact.Name" class="form-control" placeholder="Some Name"/>
            <ValidationMessage For="() => ViewModel.Contact.Name" />
        </div>

        <div class="col-12">
            <label class="form-label">Email:</label>
            <InputText aria-label="email" @bind-Value="ViewModel.Contact.Email" class="form-control" placeholder="user@domain.tld"/>
            <ValidationMessage For="() => ViewModel.Contact.Email" />
        </div>
        <div class="col-12">
            <label class="form-label">Phone Number:</label>
            <InputText aria-label="phone number" @bind-Value="ViewModel.Contact.PhoneNumber" class="form-control" placeholder="555-1212"/>
            <ValidationMessage For="() => ViewModel.Contact.PhoneNumber" />
        </div>
    </div>

    <hr class="my-4">

    <div class="row">
        <button class="btn btn-primary btn-lg col"
                type="submit"
                disabled="@ViewModel.Contact.HasErrors">
        Save
        </button>
        <button class="btn btn-secondary btn-lg col"
                type="button" 
                @onclick="ViewModel.ClearFormCommand.Execute">
            Clear Form
        </button>
    </div>
</EditForm>  
```

### Subpath Hosting

Blazing.Mvvm supports hosting your Blazor application under a subpath of a web server. This is useful when you want to serve your application from a specific URL segment rather than the root of the domain (e.g., `https://example.com/myapp` instead of `https://example.com`).

#### Configuration Steps

**1. Configure `launchSettings.json`**

Add the `launchUrl` property to specify the subpath:

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
}
```

**2. Configure Blazing.Mvvm in `Program.cs`**

Set the `BasePath` property in the `AddMvvm` configuration:

```csharp
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.Server;
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
    options.BasePath = "/fu/bar/"; // Set the base path for the application
});
```

**3. Configure ASP.NET Core Middleware**

Add the path base middleware in `Program.cs`:

```csharp
app.UsePathBase("/fu/bar/"); // Set the base path for the application

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.Run();
```

**4. Update the Base Href**

Set the `<base>` element in your `index.html` (WebAssembly) or `_Host.cshtml` (Server):

```html
<base href="/fu/bar/" />
```

For a complete working example, see the [Blazing.SubpathHosting.Server](samples/Blazing.SubpathHosting.Server) sample project.

### Complex Multi-Project ViewModel Registration

When working with complex multi-project solutions where ViewModels are distributed across multiple assemblies, you can register all ViewModels from different assemblies using the `RegisterViewModelsFromAssemblyContaining` method in the `AddMvvm` configuration.

This is particularly useful in Hybrid applications (WPF, WinForms, MAUI, Avalonia) where you might have:
- A core project containing business logic and ViewModels
- A Blazor UI project containing page-specific ViewModels
- Shared ViewModels across multiple projects

#### Example: Registering ViewModels from Multiple Assemblies

```csharp
using Blazing.Mvvm;
using HybridSample.Core.ViewModels;
using HybridSample.Blazor.Core.Pages;

builder.Services.AddMvvm(options =>
{ 
    options.HostingModelType = BlazorHostingModelType.Hybrid;
    
    // Register ViewModels from the Core project
    options.RegisterViewModelsFromAssemblyContaining<SamplePageViewModel>();
    
    // Register ViewModels from the Blazor.Core project
    options.RegisterViewModelsFromAssemblyContaining<IntroductionPage>();
});
```

#### Alternative Methods

You can also register assemblies directly:

```csharp
// Using Type
var coreAssembly = typeof(SamplePageViewModel).Assembly;
var blazorAssembly = typeof(IntroductionPage).Assembly;

builder.Services.AddMvvm(options =>
{ 
    options.RegisterViewModelsFromAssembly(coreAssembly, blazorAssembly);
});

// Or using RegisterViewModelsFromAssemblies for a collection
var assemblies = new[] { coreAssembly, blazorAssembly };
builder.Services.AddMvvm(options =>
{ 
    options.RegisterViewModelsFromAssemblies(assemblies);
});
```

This approach ensures that all ViewModels across your solution are properly discovered and registered with the dependency injection container, enabling seamless MVVM navigation and component resolution.

For working examples, see the Hybrid sample projects:
- [HybridSample.Wpf](samples/HybridSamples/HybridSample.Wpf)
- [HybridSample.WinForms](samples/HybridSamples/HybridSample.WinForms)
- [HybridSample.MAUI](samples/HybridSamples/HybridSample.MAUI)
- [HybridSample.Avalonia](samples/HybridSamples/HybridSample.Avalonia)

### Sample Projects

The repository includes several sample projects demonstrating different Blazor hosting models and scenarios:

#### Blazor Hosting Model Samples

- **[Blazing.Mvvm.Sample.Server](samples/Blazing.Mvvm.Sample.Server)** - Blazor Server App sample
- **[Blazing.Mvvm.Sample.Wasm](samples/Blazing.Mvvm.Sample.Wasm)** - Blazor WebAssembly (WASM) App sample
- **[Blazing.Mvvm.Sample.WebApp](samples/Blazing.Mvvm.Sample.WebApp)** - Blazor Web App (.NET 8+) sample
- **[Blazing.Mvvm.Sample.HybridMaui](samples/Blazing.Mvvm.Sample.HybridMaui)** - Blazor Hybrid MAUI sample

#### Blazor Hybrid Samples

Modernises the Microsoft's [Xamarin Sample](https://github.com/CommunityToolkit/MVVM-Samples) project, using Blazing.Mvvm, for the [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/).  Minimal changes were made.

- **[HybridSample.Wpf](samples/HybridSamples/HybridSample.Wpf)** - WPF Blazor Hybrid sample
- **[HybridSample.WinForms](samples/HybridSamples/HybridSample.WinForms)** - WinForms Blazor Hybrid sample
- **[HybridSample.MAUI](samples/HybridSamples/HybridSample.MAUI)** - MAUI Blazor Hybrid sample
- **[HybridSample.Avalonia](samples/HybridSamples/HybridSample.Avalonia)** - Avalonia Blazor Hybrid sample (Windows only)

**NOTE:** The original Project was [Blazor MVVM Sample](https://github.com/gragra33/MvvmSampleBlazor) - now archived.

#### Specialized Samples

- **[Blazing.SubpathHosting.Server](samples/Blazing.SubpathHosting.Server)** - Demonstrates subpath hosting configuration
- **[Blazing.Mvvm.ParentChildSample](samples/ParentChildSample)** - Demonstrates dynamic parent-child component communication using Messenger. [Original](https://github.com/gragra33/Blazing.Mvvm.ParentChildSample)  repo is now archived.

#### Running Samples with Different .NET Target Frameworks

All sample projects in this repository support multi-targeting across .NET 8, .NET 9, and .NET 10. To run a sample with a specific .NET version:

1. Open the solution in Visual Studio or your preferred IDE
2. Right-click on the sample project you want to run and `Set as Startup Project`
3. Select the Start With Debugging Run Button (green solid) dropdown arrow
4. Select the target framework from the dropdown (e.g., `net8.0`, `net9.0`, `net10.0`)
5. Run the project

For detailed instructions on switching between .NET target frameworks and troubleshooting multi-targeting scenarios, see the [Running Samples with Different .NET Versions](docs/Running_Different_NET_Versions.md) guide.

## History

### V3.0.0 - 18 November 2025

This is a major release with new features and enhancements.

- Added support for .NET 10. [@gragra33](https://github.com/gragra33)
- Added subpath hosting support for serving Blazor applications from URL subpaths. [@gragra33](https://github.com/gragra33)
- Added new sample projects:
  - `Blazing.Mvvm.ParentChildSample` - Demonstrates dynamic parent-child component communication
  - `Blazing.SubpathHosting.Server` - Demonstrates subpath hosting configuration
  - Hybrid samples for WinForms, WPF, MAUI, and Avalonia platforms
- Added multi-targeting support across .NET 8, .NET 9, and .NET 10 for all sample projects. [@gragra33](https://github.com/gragra33)
- Increased test coverage with an additional 128 unit tests (total 208 tests). [@gragra33](https://github.com/gragra33)
- Enhanced documentation with comprehensive guides for:
  - Subpath hosting configuration
  - Complex multi-project ViewModel registration
  - Running samples with different .NET target frameworks
- Documentation updates and improvements. [@gragra33](https://github.com/gragra33)

### V2.2.0 - 7 December, 2024

- Added support for `ObservableRecipient` being set to inactive when disposing the `MvvmComponentBase`, `MvvmOwningComponentBase`, `MvvmLayoutComponentBase`, and `RecipientViewModelBase`. [@gragra33](https://github.com/gragra33) & [@teunlielu](https://github.com/teunlielu)

### V2.1.1 4 December, 2024

- Version bump to fix a nuget release issue

### V2.1.0 - 3 December, 2024

- Added MAUI Blazor Hybrid App support + sample HybridMaui app. [@hakakou](https://github.com/hakakou)

### V2.0.0 - 30 November, 2024

This is a major release with breaking changes, migration notes can be found [here](docs/migration-notes/v1.4_to_v2.md).

- Added auto registration and discovery of view models. [@mishael-o](https://github.com/mishael-o)
- Added support for keyed view models. [@mishael-o](https://github.com/mishael-o)
- Added support for keyed view models to `MvvmNavLink`, `MvvmKeyNavLink` (new component), `MvvmNavigationManager`, `MvvmComponentBase`, `MvvmOwningComponentBase`, & `MvvmLayoutComponentBase`. [@gragra33](https://github.com/gragra33)
- Added a `MvvmObservableValidator` component which provides support for `ObservableValidator`. [@mishael-o](https://github.com/mishael-o)
- Added parameter resolution in the ViewModel. [@mishael-o](https://github.com/mishael-o)
- Added new `TestKeyedNavigation` samples for Keyed Navigation. [@gragra33](https://github.com/gragra33)
- Added & Updated tests for all changes made. [@mishael-o](https://github.com/mishael-o) & [@gragra33](https://github.com/gragra33)
- Added support for .NET 9. [@gragra33](https://github.com/gragra33)
- Dropped support for .NET 7. [@mishael-o](https://github.com/mishael-o)
- Documentation updates. [@mishael-o](https://github.com/mishael-o) & [@gragra33](https://github.com/gragra33)

**BREAKING CHANGES:**

- Renamed `BlazorHostingModel` to `BlazorHostingModelType` to avoid confusion

The full history can be found in the [Version Tracking](https://github.com/gragra33/Blazing.Mvvm/HISTORY.md) documentation.
