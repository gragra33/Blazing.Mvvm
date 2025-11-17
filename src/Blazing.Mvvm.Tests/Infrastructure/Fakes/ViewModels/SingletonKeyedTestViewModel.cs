using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// A test view model with singleton lifetime and a specific key for use in unit tests.
/// </summary>
/// <remarks>
/// This class is registered as a singleton service with a key using <see cref="ViewModelDefinitionAttribute"/>.
/// </remarks>
[ViewModelDefinition(Key = nameof(SingletonKeyedTestViewModel), Lifetime = ServiceLifetime.Singleton)]
internal sealed class SingletonKeyedTestViewModel : ViewModelBase;
