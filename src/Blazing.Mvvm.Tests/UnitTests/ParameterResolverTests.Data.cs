using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.UnitTests;

public partial class ParameterResolverTests
{
    private interface IParameterTestView : IView<TestParameterViewModel>
    {
        public const string NamedCascadingParameterName = "cascadingNamed";
        public const string NamedQueryParameterName = "queryNamed";

        string Parameter1 { get; set; }

        public int Parameter2 { get; set; }

        string? Parameter3 { get; set; }

        IParameterTestView ParentView { get; set; }

        TestParameterViewModel ParentViewModel { get; set; }

        string? QueryParameter { get; set; }

        DateOnly? QueryParameter2 { get; set; }
    }

    private class TestParameterView : MvvmComponentBase<TestParameterViewModel>, IParameterTestView
    {
        [Parameter]
        public string Parameter1 { get; set; } = default!;

        [Parameter]
        public int Parameter2 { get; set; }

        [Parameter]
        public string? Parameter3 { get; set; }

        [CascadingParameter]
        public IParameterTestView ParentView { get; set; } = default!;

        [CascadingParameter(Name = IParameterTestView.NamedCascadingParameterName)]
        public TestParameterViewModel ParentViewModel { get; set; } = default!;

        [SupplyParameterFromQuery]
        public string? QueryParameter { get; set; }

        [SupplyParameterFromQuery(Name = IParameterTestView.NamedQueryParameterName)]
        public DateOnly? QueryParameter2 { get; set; }
    }

    private class TestParameterLayoutView : MvvmLayoutComponentBase<TestParameterViewModel>, IParameterTestView
    {
        [Parameter]
        public string Parameter1 { get; set; } = default!;

        [Parameter]
        public int Parameter2 { get; set; }

        [Parameter]
        public string? Parameter3 { get; set; }

        [CascadingParameter]
        public IParameterTestView ParentView { get; set; } = default!;

        [CascadingParameter(Name = IParameterTestView.NamedCascadingParameterName)]
        public TestParameterViewModel ParentViewModel { get; set; } = default!;

        [SupplyParameterFromQuery]
        public string? QueryParameter { get; set; }

        [SupplyParameterFromQuery(Name = IParameterTestView.NamedQueryParameterName)]
        public DateOnly? QueryParameter2 { get; set; }
    }

    private class TestParameterOwingView : MvvmOwningComponentBase<TestParameterViewModel>, IParameterTestView
    {
        [Parameter]
        public string Parameter1 { get; set; } = default!;

        [Parameter]
        public int Parameter2 { get; set; }

        [Parameter]
        public string? Parameter3 { get; set; }

        [CascadingParameter]
        public IParameterTestView ParentView { get; set; } = default!;

        [CascadingParameter(Name = IParameterTestView.NamedCascadingParameterName)]
        public TestParameterViewModel ParentViewModel { get; set; } = default!;

        [SupplyParameterFromQuery]
        public string? QueryParameter { get; set; }

        [SupplyParameterFromQuery(Name = IParameterTestView.NamedQueryParameterName)]
        public DateOnly? QueryParameter2 { get; set; }
    }

    private class TestParameterViewModel : ViewModelBase
    {
        [ViewParameter]
        public string Parameter1 { get; set; } = default!;

        [ViewParameter("Parameter2")]
        private int Property1 { get; set; }

        [ViewParameter]
        public string? Parameter3 { get; set; }

        [ViewParameter]
        public IParameterTestView ParentView { get; private set; } = default!;

        [ViewParameter("ParentViewModel")]
        internal TestParameterViewModel AncestorViewModel { get; set; } = default!;

        [ViewParameter]
        public string? QueryParameter { get; set; }

        [ViewParameter("QueryParameter2")]
        protected DateOnly? NamedQueryParameter { get; set; }

        public int GetProperty1()
            => Property1;

        public DateOnly? GetNamedQueryParameter()
            => NamedQueryParameter;
    }

    private class TestParameterViewDuplicateKeyOnViewModel : MvvmComponentBase<TestParameterDuplicateKeyViewModel>
    {
        [Parameter]
        public string Parameter1 { get; set; } = default!;
    }

    private class TestParameterDuplicateKeyViewModel : ViewModelBase
    {
        [ViewParameter]
        public string Parameter1 { get; set; } = default!;

        [ViewParameter("Parameter1")]
        public int Property1 { get; set; }
    }

    private class TestParameterNoSetterOnViewModel : MvvmComponentBase<TestParameterNoSetterViewModel>
    {
        [Parameter]
        public string Parameter1 { get; set; } = default!;
    }

    private class TestParameterNoSetterViewModel : ViewModelBase
    {
        [ViewParameter]
        public string Parameter1 { get; } = default!;
    }
}
