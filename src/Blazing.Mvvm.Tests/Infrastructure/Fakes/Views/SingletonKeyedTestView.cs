using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes.Views;

[ViewModelKey(nameof(SingletonKeyedTestViewModel))]
[Route("/singleton-keyed")]
internal class SingletonKeyedTestView : MvvmComponentBase<SingletonKeyedTestViewModel>
{ /* skip */ }