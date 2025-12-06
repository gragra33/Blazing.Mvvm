using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.ObservablePropertyAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="ObservablePropertyAnalyzer"/>
/// </summary>
public class ObservablePropertyAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PropertyWithoutNotification_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private string _name;
        
        public string {|#0:Name|}
        {
            get => _name;
            set => _name = value; // Missing notification
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ObservablePropertyMissing)
            .WithLocation(0)
            .WithArguments("Name");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task PropertyWithSetProperty_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private string _name;
        
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task FieldWithObservableProperty_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _name;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ReadOnlyProperty_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public string Name { get; }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AutoProperty_InNonViewModelClass_NoDiagnostic()
    {
        const string test = @"
namespace TestNamespace
{
    public class TestClass
    {
        public string Name { get; set; }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PropertyWithOnPropertyChanged_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private string _name;
        
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultiplePropertiesWithoutNotification_ReportsMultipleDiagnostics()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private string _name;
        private int _age;
        
        public string {|#0:Name|}
        {
            get => _name;
            set => _name = value;
        }

        public int {|#1:Age|}
        {
            get => _age;
            set => _age = value;
        }
    }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.ObservablePropertyMissing)
            .WithLocation(0)
            .WithArguments("Name");

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.ObservablePropertyMissing)
            .WithLocation(1)
            .WithArguments("Age");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }
}
