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
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0001_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0001_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0001_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0001.md");

    // BLAZMVVM0002: ViewModelDefinition Attribute Analyzer
    public static readonly DiagnosticDescriptor ViewModelDefinitionMissing = new(
        id: "BLAZMVVM0002",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0002_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0002_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0002_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0002.md");

    // BLAZMVVM0003: MvvmComponentBase Usage Analyzer
    public static readonly DiagnosticDescriptor MvvmComponentBaseMissing = new(
        id: "BLAZMVVM0003",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0003_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0003_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0003_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0003.md");

    // BLAZMVVM0004: ViewParameter Attribute Analyzer
    public static readonly DiagnosticDescriptor ViewParameterMismatch = new(
        id: "BLAZMVVM0004",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0004_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0004_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0004_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0004.md",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    // BLAZMVVM0005: Navigation Type Safety Analyzer
    public static readonly DiagnosticDescriptor InvalidNavigationTarget = new(
        id: "BLAZMVVM0005",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0005_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0005_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0005_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0005.md");

    // BLAZMVVM0006: ViewModelKey Consistency Analyzer
    public static readonly DiagnosticDescriptor ViewModelKeyInconsistent = new(
        id: "BLAZMVVM0006",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0006_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0006_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0006_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0006.md",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    // BLAZMVVM0007: Lifecycle Method Override Analyzer
    public static readonly DiagnosticDescriptor LifecycleMethodSuggestion = new(
        id: "BLAZMVVM0007",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0007_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0007_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0007_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0007.md");

    // BLAZMVVM0008: Observable Property Analyzer
    public static readonly DiagnosticDescriptor ObservablePropertyMissing = new(
        id: "BLAZMVVM0008",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0008_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0008_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0008_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0008.md");

    // BLAZMVVM0009: Service Injection Analyzer
    public static readonly DiagnosticDescriptor ServiceNotRegistered = new(
        id: "BLAZMVVM0009",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0009_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0009_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0009_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0009.md");

    // BLAZMVVM0010: Route-ViewModel Mapping Analyzer
    public static readonly DiagnosticDescriptor PageMissingViewModel = new(
        id: "BLAZMVVM0010",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0010_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0010_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0010_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0010.md");

    // BLAZMVVM0011: MvvmNavLink Type Safety Analyzer
    public static readonly DiagnosticDescriptor MvvmNavLinkInvalidViewModel = new(
        id: "BLAZMVVM0011",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0011_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0011_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0011_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0011.md");

    // BLAZMVVM0012: Command Pattern Analyzer
    public static readonly DiagnosticDescriptor MethodShouldBeCommand = new(
        id: "BLAZMVVM0012",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0012_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0012_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0012_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0012.md");

    // BLAZMVVM0013: MvvmOwningComponentBase Usage Analyzer
    public static readonly DiagnosticDescriptor MvvmOwningComponentBaseSuggested = new(
        id: "BLAZMVVM0013",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0013_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0013_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0013_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0013.md");

    // BLAZMVVM0014: StateHasChanged Overuse Analyzer
    public static readonly DiagnosticDescriptor StateHasChangedUnnecessary = new(
        id: "BLAZMVVM0014",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0014_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0014_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0014_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0014.md");

    // BLAZMVVM0015: Dispose Pattern Analyzer
    public static readonly DiagnosticDescriptor DisposePatternMissing = new(
        id: "BLAZMVVM0015",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0015_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0015_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0015_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0015.md");

    // BLAZMVVM0016: Messenger Registration Lifetime Analyzer
    public static readonly DiagnosticDescriptor MessengerRegistrationLeakPossible = new(
        id: "BLAZMVVM0016",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0016_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0016_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0016_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0016.md");

    // BLAZMVVM0017: RelayCommand Async Pattern Analyzer
    public static readonly DiagnosticDescriptor AsyncVoidRelayCommand = new(
        id: "BLAZMVVM0017",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0017_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0017_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0017_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0017.md");

    // BLAZMVVM0018: NotifyPropertyChangedFor Analyzer
    public static readonly DiagnosticDescriptor NotifyPropertyChangedForMissing = new(
        id: "BLAZMVVM0018",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0018_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0018_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0018_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0018.md");

    // BLAZMVVM0019: CascadingParameter vs Inject Analyzer
    public static readonly DiagnosticDescriptor InjectPreferredOverCascading = new(
        id: "BLAZMVVM0019",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0019_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0019_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0019_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0019.md");

    // BLAZMVVM0020: Route Parameter Binding Analyzer
    public static readonly DiagnosticDescriptor RouteParameterBindingMissing = new(
        id: "BLAZMVVM0020",
        title: new LocalizableResourceString(nameof(Resources.BLAZMVVM0020_Title), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.BLAZMVVM0020_MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.BLAZMVVM0020_Description), Resources.ResourceManager, typeof(Resources)),
        helpLinkUri: "https://github.com/gragra33/Blazing.Mvvm/docs/analyzers/BLAZMVVM0020.md",
        customTags: WellKnownDiagnosticTags.CompilationEnd);
}
