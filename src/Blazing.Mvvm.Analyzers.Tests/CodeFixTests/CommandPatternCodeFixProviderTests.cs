using Blazing.Mvvm.Analyzers.Analyzers;
using Blazing.Mvvm.Analyzers.CodeFixProviders;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpCodeFixVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.CommandPatternAnalyzer,
    Blazing.Mvvm.Analyzers.CodeFixProviders.CommandPatternCodeFixProvider>;

namespace Blazing.Mvvm.Analyzers.Tests.CodeFixTests;

/// <summary>
/// Unit tests for <see cref="CommandPatternCodeFixProvider"/>
/// </summary>
public class CommandPatternCodeFixProviderTests
{
    [Fact]
    public async Task PublicVoidMethod_ConvertsToRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public void {|#0:Execute|}()
        {
            // Do something
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        [RelayCommand]
        private void Execute()
        {
            // Do something
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("Execute");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task PublicTaskMethod_ConvertsToRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class DataViewModel : ViewModelBase
    {
        public Task {|#0:LoadDataAsync|}()
        {
            return Task.CompletedTask;
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class DataViewModel : ViewModelBase
    {
        [RelayCommand]
        private Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("LoadDataAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task PublicAsyncTaskMethod_ConvertsToRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class SaveViewModel : ViewModelBase
    {
        public async Task {|#0:SaveAsync|}()
        {
            await Task.Delay(100);
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class SaveViewModel : ViewModelBase
    {
        [RelayCommand]
        private async Task SaveAsync()
        {
            await Task.Delay(100);
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("SaveAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task PublicValueTaskMethod_ConvertsToRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class FastViewModel : ViewModelBase
    {
        public ValueTask {|#0:ProcessAsync|}()
        {
            return ValueTask.CompletedTask;
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class FastViewModel : ViewModelBase
    {
        [RelayCommand]
        private ValueTask ProcessAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("ProcessAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task MethodWithParameters_ConvertsToRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class ItemViewModel : ViewModelBase
    {
        public void {|#0:Delete|}(int id)
        {
            // Delete item
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class ItemViewModel : ViewModelBase
    {
        [RelayCommand]
        private void Delete(int id)
        {
            // Delete item
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("Delete");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task MethodWithBody_ConvertsToRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class CounterViewModel : ViewModelBase
    {
        private int _count;

        public void {|#0:Increment|}()
        {
            _count++;
            OnPropertyChanged(nameof(_count));
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class CounterViewModel : ViewModelBase
    {
        private int _count;

        [RelayCommand]
        private void Increment()
        {
            _count++;
            OnPropertyChanged(nameof(_count));
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("Increment");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task MultiplePublicMethods_ConvertsEachToRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class FormViewModel : ViewModelBase
    {
        public void {|#0:Save|}()
        {
        }

        public Task {|#1:ValidateAsync|}()
        {
            return Task.CompletedTask;
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class FormViewModel : ViewModelBase
    {
        [RelayCommand]
        private void Save()
        {
        }

        [RelayCommand]
        private Task ValidateAsync()
        {
            return Task.CompletedTask;
        }
    }
}";

        var expectedSave = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("Save");

        var expectedValidate = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(1)
            .WithArguments("ValidateAsync");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expectedSave, expectedValidate);
    }

    [Fact]
    public async Task ViewModelWithExistingInputUsing_DoesNotDuplicateUsing()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class ExistingViewModel : ViewModelBase
    {
        public void {|#0:Execute|}()
        {
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class ExistingViewModel : ViewModelBase
    {
        [RelayCommand]
        private void Execute()
        {
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("Execute");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task RecipientViewModelBase_ConvertsToRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MessagingViewModel : RecipientViewModelBase
    {
        public void {|#0:SendMessage|}()
        {
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public class MessagingViewModel : RecipientViewModelBase
    {
        [RelayCommand]
        private void SendMessage()
        {
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("SendMessage");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }
}
