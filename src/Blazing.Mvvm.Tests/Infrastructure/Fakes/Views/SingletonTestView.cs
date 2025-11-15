using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes.Views;

[ViewModelKey(nameof(SingletonTestViewModel))]
[Route("/singleton")]
internal class SingletonTestView : MvvmComponentBase<ISingletonTestViewModel>
{ /* skip */ }