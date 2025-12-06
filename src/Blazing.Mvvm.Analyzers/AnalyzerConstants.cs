namespace Blazing.Mvvm.Analyzers;

/// <summary>
/// Constants used throughout the analyzer codebase.
/// </summary>
internal static class AnalyzerConstants
{
    /// <summary>
    /// Fully qualified type names for Blazing.Mvvm types.
    /// </summary>
    public static class TypeNames
    {
        public const string ViewModelBase = "Blazing.Mvvm.ComponentModel.ViewModelBase";
        public const string RecipientViewModelBase = "Blazing.Mvvm.ComponentModel.RecipientViewModelBase";
        public const string ValidatorViewModelBase = "Blazing.Mvvm.ComponentModel.ValidatorViewModelBase";
        public const string IViewModelBase = "Blazing.Mvvm.ComponentModel.IViewModelBase";
        
        public const string MvvmComponentBase = "Blazing.Mvvm.Components.MvvmComponentBase";
        public const string MvvmOwningComponentBase = "Blazing.Mvvm.Components.MvvmOwningComponentBase";
        public const string MvvmLayoutComponentBase = "Blazing.Mvvm.Components.MvvmLayoutComponentBase";
        
        public const string ViewModelDefinitionAttribute = "Blazing.Mvvm.ComponentModel.ViewModelDefinitionAttribute";
        public const string ViewParameterAttribute = "Blazing.Mvvm.ComponentModel.ViewParameterAttribute";
        public const string ViewModelKeyAttribute = "Blazing.Mvvm.Components.ViewModelKeyAttribute";
        
        public const string IMvvmNavigationManager = "Blazing.Mvvm.Components.IMvvmNavigationManager";
        public const string MvvmNavLink = "Blazing.Mvvm.Components.Routing.MvvmNavLink";
        public const string MvvmKeyNavLink = "Blazing.Mvvm.Components.Routing.MvvmKeyNavLink";
        
        // Blazor types
        public const string ComponentBase = "Microsoft.AspNetCore.Components.ComponentBase";
        public const string ParameterAttribute = "Microsoft.AspNetCore.Components.ParameterAttribute";
        
        // CommunityToolkit.Mvvm types
        public const string ObservablePropertyAttribute = "CommunityToolkit.Mvvm.ComponentModel.ObservablePropertyAttribute";
        public const string RelayCommandAttribute = "CommunityToolkit.Mvvm.Input.RelayCommandAttribute";
        public const string ObservableObject = "CommunityToolkit.Mvvm.ComponentModel.ObservableObject";
    }

    /// <summary>
    /// Common suffixes and naming patterns.
    /// </summary>
    public static class Naming
    {
        public const string ViewModelSuffix = "ViewModel";
        public const string ViewSuffix = "View";
        public const string PageSuffix = "Page";
    }

    /// <summary>
    /// Method names to look for in analyzers.
    /// </summary>
    public static class MethodNames
    {
        public const string NavigateTo = "NavigateTo";
        public const string SetProperty = "SetProperty";
        public const string OnInitialized = "OnInitialized";
        public const string OnInitializedAsync = "OnInitializedAsync";
        public const string OnParametersSet = "OnParametersSet";
        public const string OnParametersSetAsync = "OnParametersSetAsync";
        public const string OnAfterRender = "OnAfterRender";
        public const string OnAfterRenderAsync = "OnAfterRenderAsync";
    }

    /// <summary>
    /// Property names to look for in analyzers.
    /// </summary>
    public static class PropertyNames
    {
        public const string ViewModel = "ViewModel";
    }

    /// <summary>
    /// Attribute names (short form without 'Attribute' suffix).
    /// </summary>
    public static class AttributeNames
    {
        public const string ViewModelDefinition = "ViewModelDefinition";
        public const string ViewParameter = "ViewParameter";
        public const string ViewModelKey = "ViewModelKey";
        public const string ObservableProperty = "ObservableProperty";
        public const string RelayCommand = "RelayCommand";
    }
}
