using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelDefinition(Key = nameof(TransientKeyedTestViewModel), Lifetime = ServiceLifetime.Transient)]
internal sealed class TransientKeyedTestViewModel : ViewModelBase;
