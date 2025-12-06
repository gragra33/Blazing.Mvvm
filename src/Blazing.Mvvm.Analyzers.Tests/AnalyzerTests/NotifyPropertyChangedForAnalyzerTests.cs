using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.NotifyPropertyChangedForAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="NotifyPropertyChangedForAnalyzer"/>
/// </summary>
public class NotifyPropertyChangedForAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ComputedProperty_WithoutNotification_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string {|#0:_firstName|};

        [ObservableProperty]
        private string {|#1:_lastName|};

        // Manually declare generated properties for testing
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string FullName => $""{FirstName} {LastName}"";
    }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.NotifyPropertyChangedForMissing)
            .WithLocation(0)
            .WithArguments("_firstName", "FullName");

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.NotifyPropertyChangedForMissing)
            .WithLocation(1)
            .WithArguments("_lastName", "FullName");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task ComputedProperty_WithNotification_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(""FullName"")]
        private string _firstName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(""FullName"")]
        private string _lastName;

        // Manually declare generated properties for testing
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string FullName => $""{FirstName} {LastName}"";
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PropertyWithSetProperty_AndComputedProperty_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private int {|#0:_count|};

        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        public bool HasItems => Count > 0;
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.NotifyPropertyChangedForMissing)
            .WithLocation(0)
            .WithArguments("_count", "HasItems");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task MultipleComputedProperties_DependingOnSameProperty_ReportsMultipleDiagnostics()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private decimal {|#0:_price|};

        // Manually declare generated property for testing
        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public decimal PriceWithTax => Price * 1.2m;
        public string PriceDisplay => $""${Price:F2}"";
    }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.NotifyPropertyChangedForMissing)
            .WithLocation(0)
            .WithArguments("_price", "PriceWithTax");

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.NotifyPropertyChangedForMissing)
            .WithLocation(0)
            .WithArguments("_price", "PriceDisplay");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task PropertyNotUsedInComputedProperty_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _firstName;

        [ObservableProperty]
        private string _lastName;

        public string DisplayName => ""User"";
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task SimpleProperty_WithoutComputedDependents_NoDiagnostic()
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
    public async Task NotifyCanExecuteChangedFor_NotSameAsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(""SaveCommand"")]
        private bool _canSave;

        // Manually declare generated property for testing
        public bool CanSave
        {
            get => _canSave;
            set => SetProperty(ref _canSave, value);
        }

        [RelayCommand(CanExecute = ""CanSave"")]
        private void Save()
        {
        }

        // Manually declare generated command for testing
        public RelayCommand SaveCommand { get; }
    }
}";

        // NotifyCanExecuteChangedFor is different from NotifyPropertyChangedFor
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
