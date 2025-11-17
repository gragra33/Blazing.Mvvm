using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes.Views;

/// <summary>
/// A test view for the <see cref="ISingletonTestViewModel"/> with singleton registration.
/// Used in unit tests to verify view-model binding and routing.
/// </summary>
/// <remarks>
/// This view is associated with the <see cref="SingletonTestViewModel"/> using the <see cref="ViewModelKeyAttribute"/> and is routed to "/singleton".
/// </remarks>
[ViewModelKey(nameof(SingletonTestViewModel))]
[Route("/singleton")]
internal class SingletonTestView : MvvmComponentBase<ISingletonTestViewModel>
{ /* skip */ }