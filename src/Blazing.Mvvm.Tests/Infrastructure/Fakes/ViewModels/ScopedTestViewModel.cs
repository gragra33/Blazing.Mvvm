using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// A test view model with scoped lifetime for use in unit tests.
/// </summary>
/// <remarks>
/// This class is registered as a scoped service using <see cref="ViewModelDefinitionAttribute"/>.
/// </remarks>
[ViewModelDefinition<IScopedTestViewModel>(Lifetime = ServiceLifetime.Scoped)]
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
internal sealed class ScopedTestViewModel : ViewModelBase, IScopedTestViewModel;
