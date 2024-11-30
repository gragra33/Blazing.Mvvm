using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelKey(nameof(SingletonTestViewModel))]
[Route("/singleton")]
internal class SingletonTestView : MvvmComponentBase<ISingletonTestViewModel>
{ }

[ViewModelKey(nameof(SingletonKeyedTestViewModel))]
[Route("/singleton-keyed")]
internal class SingletonKeyedTestView : MvvmComponentBase<SingletonKeyedTestViewModel>
{ }
