using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelDefinition(Key = nameof(ScopedKeyedTestViewModel), Lifetime = ServiceLifetime.Scoped)]
internal sealed class ScopedKeyedTestViewModel : ViewModelBase;
