using Blazing.Mvvm.AnalyzerTest.Data;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.AnalyzerTest.Components.Pages;

/// <summary>
/// Intentional analyzer sample for BLAZMVVM0019.
/// Uses CascadingParameter for DI-style services.
/// </summary>
[Route("/test019-code")]
public class Test019CodeComponent : ComponentBase
{
    [CascadingParameter]
    public ITestService TestService { get; set; } = null!;

    [CascadingParameter]
    public HttpClient HttpClient { get; set; } = null!;
}
