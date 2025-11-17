using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// A test view model with singleton lifetime for use in unit tests.
/// </summary>
/// <remarks>
/// This class is registered as a singleton service using <see cref="ViewModelDefinitionAttribute"/>.
/// Multiple registrations are provided for different keys and interfaces.
/// </remarks>
[ViewModelDefinition<ISingletonTestViewModel>(Lifetime = ServiceLifetime.Singleton)]
[ViewModelDefinition<ISingletonTestViewModel>(Key = nameof(SingletonTestViewModel), Lifetime = ServiceLifetime.Singleton)]
[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
internal sealed class SingletonTestViewModel : ViewModelBase, ISingletonTestViewModel;
