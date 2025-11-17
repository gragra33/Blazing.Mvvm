using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes.Views;

/// <summary>
/// A test view for the <see cref="SingletonKeyedTestViewModel"/> with singleton keyed registration.
/// Used in unit tests to verify view-model binding and routing.
/// </summary>
/// <remarks>
/// This view is associated with the <see cref="SingletonKeyedTestViewModel"/> using the <see cref="ViewModelKeyAttribute"/> and is routed to "/singleton-keyed".
/// </remarks>
[ViewModelKey(nameof(SingletonKeyedTestViewModel))]
[Route("/singleton-keyed")]
internal class SingletonKeyedTestView : MvvmComponentBase<SingletonKeyedTestViewModel>
{ /* skip */ }