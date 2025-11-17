using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// A test view model with transient lifetime and a specific key for use in unit tests.
/// </summary>
/// <remarks>
/// This class is registered as a transient service with a key using <see cref="ViewModelDefinitionAttribute"/>.
/// </remarks>
[ViewModelDefinition(Key = nameof(TransientKeyedTestViewModel), Lifetime = ServiceLifetime.Transient)]
internal sealed class TransientKeyedTestViewModel : ViewModelBase;
