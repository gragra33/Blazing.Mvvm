using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelDefinition(Key = nameof(SingletonKeyedTestViewModel), Lifetime = ServiceLifetime.Singleton)]
internal sealed class SingletonKeyedTestViewModel : ViewModelBase;
