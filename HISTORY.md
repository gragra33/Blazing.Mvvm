# Version History

### V3.1.0 - 30 November 2025

This release adds automatic base path detection for YARP reverse proxy scenarios and simplifies configuration.

**New Features:**
- **Automatic Base Path Detection:** Base path is now automatically detected from `NavigationManager.BaseUri`, eliminating the need for manual `BasePath` configuration in most scenarios. [@gragra33](https://github.com/gragra33)
- **YARP Support:** Full support for YARP (Yet Another Reverse Proxy) with automatic detection of dynamically assigned paths via `PathBase`. [@gragra33](https://github.com/gragra33)
- **Dynamic Per-Request Base Paths:** Supports scenarios where different requests have different base paths, ideal for multi-tenant applications. [@gragra33](https://github.com/gragra33)

**Improvements:**
- `BasePath` property is now marked as `[Obsolete]` but remains functional for backward compatibility. [@gragra33](https://github.com/gragra33)
- Added 15 new unit tests and integration tests for dynamic base path scenarios (total 867 tests). [@gragra33](https://github.com/gragra33)
- Enhanced logging for base path detection to aid in diagnostics. [@gragra33](https://github.com/gragra33)
- Updated documentation with YARP configuration examples and best practices. [@gragra33](https://github.com/gragra33)

**Configuration:**
- **No configuration required** for most scenarios - base path is automatically detected
- For YARP scenarios, simply use `app.UseForwardedHeaders()` and optionally handle `X-Forwarded-Prefix` header
- Existing code using `BasePath` continues to work without changes

See the [Subpath Hosting](../readme.md#subpath-hosting) section in the readme for updated configuration examples.

### V3.0.0 - 18 November 2025

This is a major release with new features and enhancements.

- Added support for .NET 10. [@gragra33](https://github.com/gragra33)
- Added subpath hosting support for serving Blazor applications from URL subpaths. [@gragra33](https://github.com/gragra33)
- Added new sample projects:
  - `Blazing.Mvvm.ParentChildSample` - Demonstrates dynamic parent-child component communication
  - `Blazing.SubpathHosting.Server` - Demonstrates subpath hosting configuration
  - Hybrid samples for WinForms, WPF, MAUI, and Avalonia platforms
- Added multi-targeting support across .NET 8, .NET 9, and .NET 10 for all sample projects. [@gragra33](https://github.com/gragra33)
- Increased test coverage with an additional 128 unit tests (total 208 tests). [@gragra33](https://github.com/gragra33)
- Enhanced documentation with comprehensive guides for:
  - Subpath hosting configuration
  - Complex multi-project ViewModel registration
  - Running samples with different .NET target frameworks
- Documentation updates and improvements. [@gragra33](https://github.com/gragra33)

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