using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelDefinition<ISingletonTestViewModel>(Lifetime = ServiceLifetime.Singleton)]
[ViewModelDefinition<ISingletonTestViewModel>(Key = nameof(SingletonTestViewModel), Lifetime = ServiceLifetime.Singleton)]
[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
internal sealed class SingletonTestViewModel : ViewModelBase, ISingletonTestViewModel;
