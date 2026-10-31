# Analyzer Rule Guides

The [Blazing.Mvvm.Analyzers](https://www.nuget.org/packages/Blazing.Mvvm.Analyzers) package ships Roslyn analyzers that catch common MVVM mistakes at build time: the wrong base class, missing attributes, unsafe navigation calls, or a broken dispose pattern. Most rules include a one-click code fix.

| Rule | Title | Severity |
|------|-------|----------|
| [BLAZMVVM0001](BLAZMVVM0001.md) | ViewModelBase Inheritance | Warning |
| [BLAZMVVM0002](BLAZMVVM0002.md) | ViewModelDefinition Attribute | Warning |
| [BLAZMVVM0003](BLAZMVVM0003.md) | MvvmComponentBase Usage | Error |
| [BLAZMVVM0004](BLAZMVVM0004.md) | ViewParameter Attribute Usage | Warning |
| [BLAZMVVM0005](BLAZMVVM0005.md) | Navigation Type Safety | Error |
| [BLAZMVVM0006](BLAZMVVM0006.md) | ViewModelKey Consistency | Warning |
| [BLAZMVVM0007](BLAZMVVM0007.md) | Lifecycle Method Override | Info |
| [BLAZMVVM0008](BLAZMVVM0008.md) | Observable Property Usage | Warning |
| [BLAZMVVM0009](BLAZMVVM0009.md) | Service Injection | Warning |
| [BLAZMVVM0010](BLAZMVVM0010.md) | Route-ViewModel Mapping | Info |
| [BLAZMVVM0011](BLAZMVVM0011.md) | MvvmNavLink Type Safety | Error |
| [BLAZMVVM0012](BLAZMVVM0012.md) | Command Pattern | Info |
| [BLAZMVVM0013](BLAZMVVM0013.md) | MvvmOwningComponentBase Usage | Warning |
| [BLAZMVVM0014](BLAZMVVM0014.md) | StateHasChanged Overuse | Info |
| [BLAZMVVM0015](BLAZMVVM0015.md) | Dispose Pattern | Warning |
| [BLAZMVVM0016](BLAZMVVM0016.md) | Messenger Registration Lifetime | Warning |
| [BLAZMVVM0017](BLAZMVVM0017.md) | RelayCommand Async Pattern | Warning |
| [BLAZMVVM0018](BLAZMVVM0018.md) | NotifyPropertyChangedFor | Info |
| [BLAZMVVM0019](BLAZMVVM0019.md) | CascadingParameter vs Inject | Info |
| [BLAZMVVM0020](BLAZMVVM0020.md) | Route Parameter Binding | Warning |
| [BLAZMVVM0021](BLAZMVVM0021.md) | EventCallback Two-Way Binding | Info / Warning |
| [BLAZMVVM0022](BLAZMVVM0022.md) | View Disposal Bypass | Warning |

For the generated API documentation of the analyzer and code-fix classes, see the [Blazing.Mvvm.Analyzers API reference](../../api-docs/blazing-mvvm-analyzers/Blazing.Mvvm.Analyzers.yml).
