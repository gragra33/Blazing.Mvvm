using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelDefinition<IHexTranslateViewModel>]
public sealed class HexTranslateViewModel : ViewModelBase, IHexTranslateViewModel;
