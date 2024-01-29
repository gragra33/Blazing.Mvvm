namespace Blazing.Mvvm.Infrastructure;

public class LibraryConfiguration
{
    #region Constructor

    public LibraryConfiguration() { /* skip */ }

    #endregion

    public BlazorHostingModelType HostingModelType { get; set; } = BlazorHostingModelType.NotSpecified;
}