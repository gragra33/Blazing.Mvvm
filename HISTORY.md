# Version History

### Pre-release | `Updates` Branch

* Added support for `OwningComponentBase` with new base classes `MvvmOwningComponentBase<TViewModel>` &amp; `MvvmOwningComponentBase<TViewModel, TService>` - thanks to [@Stylus VB](https://www.codeproject.com/script/Membership/View.aspx?mid=6021896) for the suggestion
* All `ComponentBase` classes now support `IAsyncDisposable` & `IDisposable`
* All `ViewModelBase` classes now support `IAsyncDisposable` & `IDisposable`
* Updated documentation
* **BREAKING CHANGE:** renamed `BlazorHostingModel` to `BlazorHostingModelType` to avoid confusion

### v1.4.0 21 November, 2023

* Now officially supports .Net 8.0 & .Net 7.0

### v1.3.0 (beta) 1 November, 2023

* pre-release of .Net 8.0 RC2 `(Auto) Blazor WebApp` with new hosting model configuration support

### v1.2.1 1 November, 2023

* added .Net 7.0+ `Blazor Server App` support
* new hosting model configuration support added. Special thanks to [@bbunderson](https://github.com/bbunderson) for implementation.

### 26 October, 2023

* pre-release of .Net 7.0+ `Blazor Server App` support
* pre-release of .Net 8.0 RC2 `(Auto) Blazor WebApp` support

### v1.1.0 9 October, 2023

* Added `MvvmLayoutComponentBase` to support MVVM in the MainLayout.razor
* Updated sample project with example of `MvvmLayoutComponentBase` usage

### v1.0.2 27 July, 2023

* Fixed rare crossthread issue in MvvmComponentBase

### v1.0.2 25 July, 2023

* Added Added logging at start and end of `MvvmNavigationManager` cache generation for improved debugging experience

### v1.0.1 19 May, 2023

* Added non-generic `RecipientViewModelBase`
* Added `ValidatorViewModelBase`

### v1.0.0 10 May, 2023

* Initial release.