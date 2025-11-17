using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// Represents a test implementation of <see cref="IHexTranslateViewModel"/> for infrastructure fakes.
/// Implements standard MVVM lifecycle and state management for Hex translation scenarios in tests.
/// </summary>
[ViewModelDefinition<IHexTranslateViewModel>]
public sealed class HexTranslateViewModel : ViewModelBase, IHexTranslateViewModel;
