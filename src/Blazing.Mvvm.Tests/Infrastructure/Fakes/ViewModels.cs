using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

internal interface ITransientTestViewModel : IViewModelBase;

internal interface IScopedTestViewModel : IViewModelBase;

internal interface ISingletonTestViewModel : IViewModelBase;

internal sealed class TestViewModel : ViewModelBase;

[ViewModelDefinition<ITransientTestViewModel>]
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
internal sealed class TransientTestViewModel : ViewModelBase, ITransientTestViewModel;

[ViewModelDefinition<IScopedTestViewModel>(Lifetime = ServiceLifetime.Scoped)]
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
internal sealed class ScopedTestViewModel : ViewModelBase, IScopedTestViewModel;

[ViewModelDefinition<ISingletonTestViewModel>(Lifetime = ServiceLifetime.Singleton)]
[ViewModelDefinition<ISingletonTestViewModel>(Key = nameof(SingletonTestViewModel), Lifetime = ServiceLifetime.Singleton)]
[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
internal sealed class SingletonTestViewModel : ViewModelBase, ISingletonTestViewModel;

[ViewModelDefinition(Key = nameof(TransientKeyedTestViewModel), Lifetime = ServiceLifetime.Transient)]
internal sealed class TransientKeyedTestViewModel : ViewModelBase;

[ViewModelDefinition(Key = nameof(ScopedKeyedTestViewModel), Lifetime = ServiceLifetime.Scoped)]
internal sealed class ScopedKeyedTestViewModel : ViewModelBase;

[ViewModelDefinition(Key = nameof(SingletonKeyedTestViewModel), Lifetime = ServiceLifetime.Singleton)]
internal sealed class SingletonKeyedTestViewModel : ViewModelBase;

internal abstract class AbstractBaseViewModel : ViewModelBase;

[ViewModelDefinition<AbstractBaseViewModel>]
internal sealed class ConcreteViewModel : AbstractBaseViewModel;
