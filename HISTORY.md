# Version History

### V2.2.0 7 December, 2024

- Added support for `ObservableRecipient` being set to inactive when disposing the `MvvmComponentBase`, `MvvmOwningComponentBase`, `MvvmLayoutComponentBase`, and `RecipientViewModelBase`. [@gragra33](https://github.com/gragra33) & [@teunlielu](https://github.com/teunlielu)

### V2.1.1 4 December, 2024

- Version bump to fix a nuget release issue

### V2.1.0 3 December, 2024

- Added MAUI Blazor Hybrid App support + sample HybridMaui app. [@hakakou](https://github.com/hakakou)

### V2.0.0 30 November, 2024

This is a major release with breaking changes, migration notes can be found [here](docs/migration-notes/v1.4_to_v2.md).

- Added auto registration and discovery of view models. [@mishael-o](https://github.com/mishael-o)
- Added support for keyed view models. [@mishael-o](https://github.com/mishael-o)
- Added support for keyed view models to `MvvmNavLink`, `MvvmKeyNavLink` (new component), `MvvmNavigationManager`, `MvvmComponentBase`, `MvvmOwningComponentBase`, & `MvvmLayoutComponentBase`. [@gragra33](https://github.com/gragra33)
- Added a `MvvmObservableValidator` component which provides support for `ObservableValidator`. [@mishael-o](https://github.com/mishael-o)
- Added parameter resolution in the ViewModel. [@mishael-o](https://github.com/mishael-o)
- Added new `TestKeyedNavigation` samples for Keyed Navigation. [@gragra33](https://github.com/gragra33)
- Added & Updated tests for all changes made. [@mishael-o](https://github.com/mishael-o) & [@gragra33](https://github.com/gragra33)
- Added support for .NET 9. [@gragra33](https://github.com/gragra33)
- Dropped support for .NET 7. [@mishael-o](https://github.com/mishael-o)
- Documentation updates. [@mishael-o](https://github.com/mishael-o) & [@gragra33](https://github.com/gragra33)

**BREAKING CHANGES:**
- Renamed `BlazorHostingModel` to `BlazorHostingModelType` to avoid confusion

### v1.4.0 21 November, 2023

- Now officially supports .Net 8.0 & .Net 7.0

### v1.3.0 (beta) 1 November, 2023

- pre-release of .Net 8.0 RC2 `(Auto) Blazor WebApp` with new hosting model configuration support

### v1.2.1 1 November, 2023

- added .Net 7.0+ `Blazor Server App` support
- new hosting model configuration support added. Special thanks to [@bbunderson](https://github.com/bbunderson) for implementation.

### 26 October, 2023

- pre-release of .Net 7.0+ `Blazor Server App` support
- pre-release of .Net 8.0 RC2 `(Auto) Blazor WebApp` support

### v1.1.0 9 October, 2023

- Added `MvvmLayoutComponentBase` to support MVVM in the MainLayout.razor
- Updated sample project with example of `MvvmLayoutComponentBase` usage

### v1.0.2 27 July, 2023

- Fixed rare crossthread issue in MvvmComponentBase

### v1.0.2 25 July, 2023

- Added Added logging at start and end of `MvvmNavigationManager` cache generation for improved debugging experience

### v1.0.1 19 May, 2023

- Added non-generic `RecipientViewModelBase`
- Added `ValidatorViewModelBase`

### v1.0.0 10 May, 2023-
- Initial release.