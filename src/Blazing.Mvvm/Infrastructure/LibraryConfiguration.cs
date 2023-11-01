namespace Blazing.Mvvm.Infrastructure;

public class LibraryConfiguration
{
    #region Constructor

    public LibraryConfiguration() { /* skip */ }

    #endregion

    public BlazorHostingModel HostingModel { get; set; } = BlazorHostingModel.NotSpecified;
}