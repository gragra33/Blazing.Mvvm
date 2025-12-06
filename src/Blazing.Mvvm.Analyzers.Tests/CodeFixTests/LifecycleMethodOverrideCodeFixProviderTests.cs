using Blazing.Mvvm.Analyzers.Analyzers;
using Blazing.Mvvm.Analyzers.CodeFixProviders;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpCodeFixVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.LifecycleMethodOverrideAnalyzer,
    Blazing.Mvvm.Analyzers.CodeFixProviders.LifecycleMethodOverrideCodeFixProvider>;

namespace Blazing.Mvvm.Analyzers.Tests.CodeFixTests;

/// <summary>
/// Unit tests for <see cref="LifecycleMethodOverrideCodeFixProvider"/>
/// </summary>
public class LifecycleMethodOverrideCodeFixProviderTests
{
    [Fact]
    public async Task ConstructorWithLogic_AddsOnInitializedAsyncOverride()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private readonly string _data;

        public {|#0:TestViewModel|}()
        {
            _data = ""initialized"";
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private readonly string _data;

        public TestViewModel()
        {
            _data = ""initialized"";
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.LifecycleMethodSuggestion)
            .WithLocation(0)
            .WithArguments("OnInitializedAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task ConstructorWithMultipleStatements_AddsOnInitializedAsyncOverride()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class DataViewModel : ViewModelBase
    {
        private int _count;
        private string _message;

        public {|#0:DataViewModel|}()
        {
            _count = 0;
            _message = ""Hello"";
            LoadDefaults();
        }

        private void LoadDefaults()
        {
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class DataViewModel : ViewModelBase
    {
        private int _count;
        private string _message;

        public DataViewModel()
        {
            _count = 0;
            _message = ""Hello"";
            LoadDefaults();
        }

        private void LoadDefaults()
        {
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.LifecycleMethodSuggestion)
            .WithLocation(0)
            .WithArguments("OnInitializedAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task ConstructorWithDependencyInjection_AddsOnInitializedAsyncOverride()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public interface IMyService { }

    public class ServiceViewModel : ViewModelBase
    {
        private readonly IMyService _service;

        public {|#0:ServiceViewModel|}(IMyService service)
        {
            _service = service;
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public interface IMyService { }

    public class ServiceViewModel : ViewModelBase
    {
        private readonly IMyService _service;

        public ServiceViewModel(IMyService service)
        {
            _service = service;
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.LifecycleMethodSuggestion)
            .WithLocation(0)
            .WithArguments("OnInitializedAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task RecipientViewModelBaseWithConstructorLogic_AddsOnInitializedAsyncOverride()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MessagingViewModel : RecipientViewModelBase
    {
        private string _title;

        public {|#0:MessagingViewModel|}()
        {
            _title = ""Messaging"";
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class MessagingViewModel : RecipientViewModelBase
    {
        private string _title;

        public MessagingViewModel()
        {
            _title = ""Messaging"";
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.LifecycleMethodSuggestion)
            .WithLocation(0)
            .WithArguments("OnInitializedAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task ValidatorViewModelBaseWithConstructorLogic_AddsOnInitializedAsyncOverride()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class FormViewModel : ValidatorViewModelBase
    {
        private bool _isValid;

        public {|#0:FormViewModel|}()
        {
            _isValid = true;
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class FormViewModel : ValidatorViewModelBase
    {
        private bool _isValid;

        public FormViewModel()
        {
            _isValid = true;
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.LifecycleMethodSuggestion)
            .WithLocation(0)
            .WithArguments("OnInitializedAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task ViewModelWithExistingUsing_DoesNotDuplicateUsing()
    {
        const string test = @"
using System.Threading.Tasks;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class ExistingUsingViewModel : ViewModelBase
    {
        private string _data;

        public {|#0:ExistingUsingViewModel|}()
        {
            _data = ""test"";
        }
    }
}";

        const string fixedCode = @"
using System.Threading.Tasks;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class ExistingUsingViewModel : ViewModelBase
    {
        private string _data;

        public ExistingUsingViewModel()
        {
            _data = ""test"";
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.LifecycleMethodSuggestion)
            .WithLocation(0)
            .WithArguments("OnInitializedAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }
}
