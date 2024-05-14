using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.Wasm.ViewModels;

#if NET8_0_OR_GREATER
[ViewModelDefinition(Key = nameof(HexTranslateViewModel))]
#endif
public class HexTranslateViewModel : ViewModelBase
{
}