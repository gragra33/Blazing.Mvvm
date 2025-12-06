using Microsoft.CodeAnalysis;

namespace Blazing.Mvvm.Analyzers;

/// <summary>
/// Diagnostic descriptors for all Blazing.Mvvm analyzers.
/// </summary>
public static class DiagnosticDescriptors
{
    private const string Category = "Blazing.Mvvm";

    // BLAZMVVM0001: ViewModelBase Inheritance Analyzer
    public static readonly DiagnosticDescriptor ViewModelBaseMissing = new(
        id: "BLAZMVVM0001",
        title: "ViewModel should inherit from ViewModelBase",
        messageFormat: "Type '{0}' should inherit from ViewModelBase, RecipientViewModelBase, or ValidatorViewModelBase",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Classes ending with 'ViewModel' should inherit from one of the Blazing.Mvvm base classes.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0001.md");

    // BLAZMVVM0002: ViewModelDefinition Attribute Analyzer
    public static readonly DiagnosticDescriptor ViewModelDefinitionMissing = new(
        id: "BLAZMVVM0002",
        title: "ViewModel should have ViewModelDefinition attribute",
        messageFormat: "ViewModel '{0}' should have [ViewModelDefinition] attribute for proper DI registration",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ViewModels should be decorated with [ViewModelDefinition] attribute to enable automatic service registration.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0002.md");

    // BLAZMVVM0003: MvvmComponentBase Usage Analyzer
    public static readonly DiagnosticDescriptor MvvmComponentBaseMissing = new(
        id: "BLAZMVVM0003",
        title: "Blazor component should inherit from MvvmComponentBase",
        messageFormat: "Component '{0}' should inherit from MvvmComponentBase<TViewModel> when using MVVM pattern",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Blazor components using ViewModels should inherit from MvvmComponentBase<TViewModel> for proper binding.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0003.md");

    // BLAZMVVM0004: ViewParameter Attribute Analyzer
    public static readonly DiagnosticDescriptor ViewParameterMismatch = new(
        id: "BLAZMVVM0004",
        title: "ViewParameter property should have corresponding Parameter in View",
        messageFormat: "Property '{0}' marked with [ViewParameter] should have a corresponding [Parameter] property in the View component",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Properties marked with [ViewParameter] in ViewModels should have matching [Parameter] properties in their View components.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0004.md",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    // BLAZMVVM0005: Navigation Type Safety Analyzer
    public static readonly DiagnosticDescriptor InvalidNavigationTarget = new(
        id: "BLAZMVVM0005",
        title: "NavigateTo references invalid ViewModel",
        messageFormat: "NavigateTo<{0}>() references a ViewModel without a valid route mapping",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "NavigateTo<TViewModel>() calls should reference ViewModels with valid @page directives or route mappings.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0005.md");

    // BLAZMVVM0006: ViewModelKey Consistency Analyzer
    public static readonly DiagnosticDescriptor ViewModelKeyInconsistent = new(
        id: "BLAZMVVM0006",
        title: "ViewModelKey attribute value should match navigation keys",
        messageFormat: "ViewModelKey attribute value '{0}' should match the key used in NavigateTo(key) calls",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ViewModelKey attribute values should be consistent with keys used in keyed navigation.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0006.md",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    // BLAZMVVM0007: Lifecycle Method Override Analyzer
    public static readonly DiagnosticDescriptor LifecycleMethodSuggestion = new(
        id: "BLAZMVVM0007",
        title: "Consider overriding lifecycle methods",
        messageFormat: "Consider overriding lifecycle method '{0}' for initialization logic",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Suggest overriding appropriate lifecycle methods (OnInitializedAsync, OnParametersSetAsync, etc.) for ViewModel initialization.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0007.md");

    // BLAZMVVM0008: Observable Property Analyzer
    public static readonly DiagnosticDescriptor ObservablePropertyMissing = new(
        id: "BLAZMVVM0008",
        title: "Property should use ObservableProperty or SetProperty",
        messageFormat: "Property '{0}' that notifies UI should use [ObservableProperty] attribute or call SetProperty",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Properties that notify the UI should use [ObservableProperty] attribute or properly call SetProperty in the setter.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0008.md");

    // BLAZMVVM0009: Service Injection Analyzer
    public static readonly DiagnosticDescriptor ServiceNotRegistered = new(
        id: "BLAZMVVM0009",
        title: "Service should be registered in DI container",
        messageFormat: "Service '{0}' injected in constructor should be registered in the DI container",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Constructor parameters should be registered services in the dependency injection container.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0009.md");

    // BLAZMVVM0010: Route-ViewModel Mapping Analyzer
    public static readonly DiagnosticDescriptor PageMissingViewModel = new(
        id: "BLAZMVVM0010",
        title: "Page should have corresponding ViewModel",
        messageFormat: "Blazor page '{0}' should have a corresponding ViewModel",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Blazor pages with @page directive should have corresponding ViewModels following MVVM pattern.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0010.md");

    // BLAZMVVM0011: MvvmNavLink Type Safety Analyzer
    public static readonly DiagnosticDescriptor MvvmNavLinkInvalidViewModel = new(
        id: "BLAZMVVM0011",
        title: "MvvmNavLink references invalid ViewModel",
        messageFormat: "MvvmNavLink TViewModel parameter references '{0}' which is not a valid registered ViewModel",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "MvvmNavLink TViewModel parameter should reference a valid registered ViewModel with a route mapping.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0011.md");

    // BLAZMVVM0012: Command Pattern Analyzer
    public static readonly DiagnosticDescriptor MethodShouldBeCommand = new(
        id: "BLAZMVVM0012",
        title: "Method should be exposed as RelayCommand",
        messageFormat: "Method '{0}' called from UI should be exposed as [RelayCommand] instead of direct method call",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Methods invoked from UI components should be exposed as commands using [RelayCommand] attribute.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0012.md");

    // BLAZMVVM0013: MvvmOwningComponentBase Usage Analyzer
    public static readonly DiagnosticDescriptor MvvmOwningComponentBaseSuggested = new(
        id: "BLAZMVVM0013",
        title: "Component should use MvvmOwningComponentBase for scoped services",
        messageFormat: "Component '{0}' should inherit from MvvmOwningComponentBase<TViewModel> when ViewModel uses scoped services like DbContext",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Components should use MvvmOwningComponentBase when their ViewModels inject scoped services to ensure proper service lifetime.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0013.md");

    // BLAZMVVM0014: StateHasChanged Overuse Analyzer
    public static readonly DiagnosticDescriptor StateHasChangedUnnecessary = new(
        id: "BLAZMVVM0014",
        title: "StateHasChanged call may be unnecessary",
        messageFormat: "StateHasChanged() call may be unnecessary when using [ObservableProperty] or SetProperty",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Manual StateHasChanged calls are unnecessary when using proper property notification with [ObservableProperty] or SetProperty.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0014.md");

    // BLAZMVVM0015: Dispose Pattern Analyzer
    public static readonly DiagnosticDescriptor DisposePatternMissing = new(
        id: "BLAZMVVM0015",
        title: "ViewModel should implement IDisposable",
        messageFormat: "ViewModel '{0}' should implement IDisposable to clean up event subscriptions and resources",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ViewModels with event subscriptions, messenger registrations, or disposable resources should implement IDisposable.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0015.md");

    // BLAZMVVM0016: Messenger Registration Lifetime Analyzer
    public static readonly DiagnosticDescriptor MessengerRegistrationLeakPossible = new(
        id: "BLAZMVVM0016",
        title: "Messenger registration without unregistration",
        messageFormat: "Messenger.Register call in '{0}' should have corresponding Unregister call to prevent memory leaks",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Messenger registrations should be properly unregistered in Dispose or use OnActivated pattern in RecipientViewModelBase.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0016.md");

    // BLAZMVVM0017: RelayCommand Async Pattern Analyzer
    public static readonly DiagnosticDescriptor AsyncVoidRelayCommand = new(
        id: "BLAZMVVM0017",
        title: "RelayCommand method should not be async void",
        messageFormat: "Method '{0}' marked with [RelayCommand] should be async Task instead of async void",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Async methods with [RelayCommand] should return Task instead of void to enable proper error handling and use AsyncRelayCommand.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0017.md");

    // BLAZMVVM0018: NotifyPropertyChangedFor Analyzer
    public static readonly DiagnosticDescriptor NotifyPropertyChangedForMissing = new(
        id: "BLAZMVVM0018",
        title: "Missing NotifyPropertyChangedFor attribute",
        messageFormat: "Property '{0}' should have [NotifyPropertyChangedFor(nameof({1}))] attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Observable properties that affect computed properties should notify using [NotifyPropertyChangedFor] attribute.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0018.md");

    // BLAZMVVM0019: CascadingParameter vs Inject Analyzer
    public static readonly DiagnosticDescriptor InjectPreferredOverCascading = new(
        id: "BLAZMVVM0019",
        title: "Use Inject instead of CascadingParameter for services",
        messageFormat: "Property '{0}' should use [Inject] instead of [CascadingParameter] for DI services",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Use [Inject] attribute for dependency injection services instead of [CascadingParameter] for better clarity.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0019.md");

    // BLAZMVVM0020: Route Parameter Binding Analyzer
    public static readonly DiagnosticDescriptor RouteParameterBindingMissing = new(
        id: "BLAZMVVM0020",
        title: "Route parameter missing corresponding property",
        messageFormat: "Route parameter '{0}' should have corresponding [Parameter] or [ViewParameter] property",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Route parameters in @page directive should have matching [Parameter] properties in View or [ViewParameter] properties in ViewModel.",
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0020.md",
        customTags: WellKnownDiagnosticTags.CompilationEnd);
}
