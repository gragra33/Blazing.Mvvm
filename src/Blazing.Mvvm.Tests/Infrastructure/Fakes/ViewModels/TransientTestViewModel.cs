using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// A test view model with transient lifetime for use in unit tests.
/// </summary>
/// <remarks>
/// This class is registered as a transient service using <see cref="ViewModelDefinitionAttribute"/>.
/// </remarks>
[ViewModelDefinition<ITransientTestViewModel>]
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
internal sealed class TransientTestViewModel : ViewModelBase, ITransientTestViewModel;
