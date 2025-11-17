using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// Represents a scoped keyed test view model for infrastructure fakes.
/// Registered with a scoped lifetime and a key for DI scenarios.
/// </summary>
[ViewModelDefinition(Key = nameof(ScopedKeyedTestViewModel), Lifetime = ServiceLifetime.Scoped)]
internal sealed class ScopedKeyedTestViewModel : ViewModelBase;
