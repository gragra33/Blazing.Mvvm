using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelDefinition<IScopedTestViewModel>(Lifetime = ServiceLifetime.Scoped)]
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
internal sealed class ScopedTestViewModel : ViewModelBase, IScopedTestViewModel;
