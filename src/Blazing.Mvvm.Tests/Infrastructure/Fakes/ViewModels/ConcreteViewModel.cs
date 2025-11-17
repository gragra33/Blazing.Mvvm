using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// Represents a concrete test view model inheriting from <see cref="AbstractBaseViewModel"/> for infrastructure fakes.
/// </summary>
[ViewModelDefinition<AbstractBaseViewModel>]
internal sealed class ConcreteViewModel : AbstractBaseViewModel;
