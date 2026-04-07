using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.AnalyzerTest.Components.Pages;

/// <summary>
/// Intentional analyzer sample for BLAZMVVM0010.
/// Route component without MVVM base or matching ViewModel.
/// </summary>
[Route("/test010-code")]
public class Test010CodePage : ComponentBase
{
    public string Title { get; set; } = "Test010 code page";
}
