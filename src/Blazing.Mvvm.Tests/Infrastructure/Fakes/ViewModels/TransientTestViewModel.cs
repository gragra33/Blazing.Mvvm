using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelDefinition<ITransientTestViewModel>]
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
internal sealed class TransientTestViewModel : ViewModelBase, ITransientTestViewModel;
