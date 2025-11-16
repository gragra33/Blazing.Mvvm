# Running Sample Projects with Different .NET Target Frameworks

This document explains how to run the sample projects targeting different versions of .NET (8.0, 9.0, and 10.0).

## Table of Contents

1. [Available Target Frameworks](#available-target-frameworks)
2. [How to Select Framework Versions](#how-to-select-framework-versions)
   - [Method 1: Using Visual Studio (Recommended)](#method-1-using-visual-studio-recommended)
   - [Method 2: Using .NET CLI](#method-2-using-net-cli-alternative-method)
3. [Running Different Project Types](#running-different-project-types)
   - [Blazor Web Applications](#blazor-web-applications)
   - [Desktop Hybrid Applications](#desktop-hybrid-applications)
   - [MAUI Cross-Platform Applications](#maui-cross-platform-applications)
4. [Understanding Multi-Targeted Projects](#understanding-multi-targeted-projects)
5. [Visual Studio UI Guide](#visual-studio-ui-guide)
6. [Prerequisites](#prerequisites)
7. [Common Issues and Solutions](#common-issues-and-solutions)
8. [Package Versioning](#package-versioning)
9. [Testing and Validation](#testing-and-validation)

## Available Target Frameworks

The sample projects in this solution support multi-targeting across the following frameworks:

### Core Library Projects
- **Blazing.Mvvm**: `net8.0`, `net9.0`, `net10.0`
- **Blazing.Mvvm.Base**: `net8.0`, `net9.0`, `net10.0`

### Blazor Web Sample Projects
- **Blazing.Mvvm.Sample.Wasm** (WebAssembly): `net8.0`, `net9.0`, `net10.0`
- **Blazing.Mvvm.Sample.Server** (Server-side): `net8.0`, `net9.0`, `net10.0`  
- **Blazing.Mvvm.Sample.WebApp** (Blazor Web App): `net8.0`, `net9.0`, `net10.0`
- **Blazing.SubpathHosting.Server** (Subpath Hosting): `net8.0`, `net9.0`, `net10.0`

### Desktop Hybrid Sample Projects
- **HybridSample.Wpf** (WPF + Blazor): `net8.0-windows`, `net9.0-windows`, `net10.0-windows`
- **HybridSample.Avalonia** (Avalonia + Blazor): `net8.0-windows`, `net9.0-windows`, `net10.0-windows`

#### Supporting Libraries for Desktop Hybrid
- **HybridSample.Core**: `net8.0`, `net9.0`, `net10.0`
- **HybridSample.Blazor.Core**: `net8.0`, `net9.0`, `net10.0`
- **Blazing.Common**: `net8.0`, `net9.0`, `net10.0`
- **Blazing.Lists**: `net8.0`, `net9.0`, `net10.0`
- **Blazing.Tabs**: `net8.0`, `net9.0`, `net10.0`
- **Baksteen.Avalonia.Blazor**: `net8.0-windows`, `net9.0-windows`, `net10.0-windows`

### MAUI Cross-Platform Sample Projects
- **Blazing.Mvvm.Sample.HybridMaui**: Multiple targets including:
  - **.NET 9.0**: `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows10.0.19041.0`
  - **.NET 10.0**: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`

## How to Select Framework Versions

### Method 1: Using Visual Studio

#### **Start With Debugging Dropdown Button**

**ALL projects in this solution are multi-targeted** and build for multiple frameworks simultaneously. To **select which specific framework to run**:

1. **Right-click on the project** you want to run in Solution Explorer
2. **Select "Set as Startup Project"**
3. **Look for the Start With Debugging Dropdown Run Button** (green solid) in the **Visual Studio toolbar**:
   - **Location**: Next to the Start Without Debugging Run (green hollow) button
4. **Select your desired target framework/platform from the dropdown**
5. **Press F5 or click the Run button**

#### **Editing Target Framework Lists**

To **modify which frameworks a project builds for**:

1. **Right-click on the project** -> **Properties** -> **Application** tab
2. **Look for "Target frameworks" field** (plural) - you can edit the semicolon-separated list here
3. **Or manually edit the .csproj file** and modify the `<TargetFrameworks>` property
4. **Reload the project** after making changes

### Method 2: Using .NET CLI (Alternative Method)

You can specify the exact target framework when building or running projects using the `-f` or `--framework` parameter:

```bash
# Build for specific framework
dotnet build -f <target-framework>

# Run for specific framework  
dotnet run -f <target-framework>
```

**Common Framework Identifiers:**
- **Standard .NET**: `net8.0`, `net9.0`, `net10.0`
- **Windows-specific**: `net8.0-windows`, `net9.0-windows`, `net10.0-windows`
- **Android**: `net9.0-android`, `net10.0-android`
- **iOS**: `net9.0-ios`, `net10.0-ios`
- **macOS**: `net9.0-maccatalyst`, `net10.0-maccatalyst`
- **Windows (MAUI)**: `net9.0-windows10.0.19041.0`, `net10.0-windows10.0.19041.0`

## Running Different Project Types

### Blazor Web Applications

Blazor web applications are the easiest to run and test across different .NET versions.

#### **WebAssembly (Client-side Blazor)**

**Project**: `Blazing.Mvvm.Sample.Wasm`  
**Target Frameworks**: `net8.0`, `net9.0`, `net10.0`

##### Visual Studio:
1. Set `Blazing.Mvvm.Sample.Wasm` as startup project
2. Select framework from Run with Debugging toolbar dropdown button: `net8.0`, `net9.0`, or `net10.0`
3. Press F5 to run

##### CLI:
```bash
cd samples/Blazing.Mvvm.Sample.Wasm

# Run .NET 8.0
dotnet run -f net8.0

# Run .NET 9.0
dotnet run -f net9.0

# Run .NET 10.0  
dotnet run -f net10.0
```

#### **Server-side Blazor**

**Project**: `Blazing.Mvvm.Sample.Server`  
**Target Frameworks**: `net8.0`, `net9.0`, `net10.0`

##### Visual Studio:
1. Set `Blazing.Mvvm.Sample.Server` as startup project
2. Select framework from Run with Debugging toolbar dropdown button: `net8.0`, `net9.0`, or `net10.0`
3. Press F5 to run

##### CLI:
```bash
cd samples/Blazing.Mvvm.Sample.Server

# Run .NET 8.0
dotnet run -f net8.0

# Run .NET 9.0
dotnet run -f net9.0

# Run .NET 10.0
dotnet run -f net10.0
```

#### **Blazor Web App (.NET 8+)**

**Project**: `Blazing.Mvvm.Sample.WebApp`  
**Target Frameworks**: `net8.0`, `net9.0`, `net10.0`

##### Visual Studio:
1. Set `Blazing.Mvvm.Sample.WebApp` as startup project
2. Select framework from Run with Debugging toolbar dropdown button: `net8.0`, `net9.0`, or `net10.0`
3. Press F5 to run

##### CLI:
```bash
cd samples/Blazing.Mvvm.Sample.WebApp/Blazing.Mvvm.Sample.WebApp

# Run .NET 8.0
dotnet run -f net8.0

# Run .NET 9.0
dotnet run -f net9.0

# Run .NET 10.0
dotnet run -f net10.0
```

#### **Subpath Hosting Server**

**Project**: `Blazing.SubpathHosting.Server`  
**Target Frameworks**: `net8.0`, `net9.0`, `net10.0`

##### Visual Studio:
1. Set `Blazing.SubpathHosting.Server` as startup project
2. Select framework from Run with Debugging toolbar dropdown button: `net8.0`, `net9.0`, or `net10.0`
3. Press F5 to run

##### CLI:
```bash
cd samples/Blazing.SubpathHosting.Server

# Run .NET 8.0
dotnet run -f net8.0

# Run .NET 9.0
dotnet run -f net9.0

# Run .NET 10.0
dotnet run -f net10.0
```

### Desktop Hybrid Applications

Desktop hybrid applications combine native desktop UI frameworks (WPF/Avalonia) with Blazor components using WebView.

#### **WPF + Blazor Hybrid**

**Project**: `HybridSample.Wpf`  
**Target Frameworks**: `net8.0-windows`, `net9.0-windows`, `net10.0-windows`

##### Visual Studio:
1. Set `HybridSample.Wpf` as startup project  
2. Select framework from Run with Debugging toolbar dropdown button: `net8.0-windows`, `net9.0-windows`, or `net10.0-windows`
3. Press F5 to run

##### CLI:
```bash
cd samples/HybridSamples/HybridSample.Wpf

# Run .NET 8.0 on Windows
dotnet run -f net8.0-windows

# Run .NET 9.0 on Windows
dotnet run -f net9.0-windows

# Run .NET 10.0 on Windows
dotnet run -f net10.0-windows
```

#### **Avalonia + Blazor Hybrid**

**Project**: `HybridSample.Avalonia`  
**Target Frameworks**: `net8.0-windows`, `net9.0-windows`, `net10.0-windows`

##### Visual Studio:
1. Set `HybridSample.Avalonia` as startup project
2. Select framework from Run with Debugging toolbar dropdown button: `net8.0-windows`, `net9.0-windows`, or `net10.0-windows`  
3. Press F5 to run

##### CLI:
```bash
cd samples/HybridSamples/HybridSample.Avalonia

# Run .NET 8.0 on Windows
dotnet run -f net8.0-windows

# Run .NET 9.0 on Windows
dotnet run -f net9.0-windows

# Run .NET 10.0 on Windows  
dotnet run -f net10.0-windows
```

### MAUI Cross-Platform Applications

MAUI applications are the most complex as they target multiple platforms simultaneously.

#### **MAUI Blazor Hybrid**

**Project**: `Blazing.Mvvm.Sample.HybridMaui`  
**Target Frameworks**: 
- **.NET 9.0**: `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows10.0.19041.0`
- **.NET 10.0**: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`

##### Visual Studio:
1. Set `Blazing.Mvvm.Sample.HybridMaui` as startup project
2. Select platform and framework from toolbar dropdown:
   - **Android**: `net9.0-android`, `net10.0-android`
   - **iOS**: `net9.0-ios`, `net10.0-ios` (macOS only)
   - **macOS**: `net9.0-maccatalyst`, `net10.0-maccatalyst` (macOS only)
   - **Windows**: `net9.0-windows10.0.19041.0`, `net10.0-windows10.0.19041.0`
3. Press F5 to run (will deploy to selected platform)

##### CLI:

**Android:**
```bash
cd samples/Blazing.Mvvm.Sample.HybridMaui

# Build for .NET 9.0 Android
dotnet build -f net9.0-android

# Build for .NET 10.0 Android
dotnet build -f net10.0-android
```

**iOS (macOS only):**
```bash
cd samples/Blazing.Mvvm.Sample.HybridMaui

# Build for .NET 9.0 iOS
dotnet build -f net9.0-ios

# Build for .NET 10.0 iOS
dotnet build -f net10.0-ios
```

**macOS Catalyst (macOS only):**
```bash
cd samples/Blazing.Mvvm.Sample.HybridMaui

# Build for .NET 9.0 macOS
dotnet build -f net9.0-maccatalyst

# Build for .NET 10.0 macOS
dotnet build -f net10.0-maccatalyst
```

**Windows:**
```bash
cd samples/Blazing.Mvvm.Sample.HybridMaui

# Run .NET 9.0 Windows
dotnet run -f net9.0-windows10.0.19041.0

# Run .NET 10.0 Windows
dotnet run -f net10.0-windows10.0.19041.0
```

## Understanding Multi-Targeted Projects

### **All Projects in This Solution are Multi-Targeted**
- **Project File Property**: Uses `<TargetFrameworks>` (plural)
- **Behavior**: Build for ALL target frameworks simultaneously
- **Visual Studio Running**: Use Start With Debugging Dropdown Button in toolbar
- **Visual Studio Editing**: Properties -> Application -> "Target frameworks" (plural) to modify the list
- **Examples**:
  - **Blazor**: `<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>`
  - **WPF/Avalonia**: `<TargetFrameworks>net8.0-windows;net9.0-windows;net10.0-windows</TargetFrameworks>`
  - **MAUI**: `<TargetFrameworks>net9.0-android;net9.0-ios;net10.0-android;net10.0-ios</TargetFrameworks>`
  - **Core Libraries**: `<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>`

## Visual Studio UI Guide

### **Start With Debugging Dropdown Button (Toolbar)**
- **Location**: Visual Studio toolbar, next to Start Without Debugging Run button button
- **Purpose**: **Select which framework to RUN** from multi-targeted projects
- **Appears when**: Multi-targeted project is set as startup project
- **Examples**:
  - **Blazor**: `net8.0`, `net9.0`, `net10.0`
  - **WPF/Avalonia**: `net8.0-windows`, `net9.0-windows`, `net10.0-windows`
  - **MAUI**: `net9.0-android`, `net9.0-windows10.0.19041.0`, `net10.0-android`, `net10.0-windows10.0.19041.0`

### **"Target frameworks" (Properties)**
- **Location**: Project Properties -> Application tab  
- **Purpose**: **Edit the semicolon-separated list of target frameworks** the project builds for
- **Shows**: Editable text field with frameworks like `net8.0;net9.0;net10.0`
- **Note**: Shows "Target frameworks" (plural) for all projects in this solution

### **Quick Start Guide**

#### **To Run a Specific Framework Version:**
1. **Set project as startup project** (right-click -> "Set as Startup Project")
2. **Find the Start Without Debugging Run button** in Visual Studio toolbar
3. **Select your desired framework** (e.g., `net8.0`, `net9.0`, `net10.0-android`)
4. **Press F5 or click Run**

#### **To Change Which Frameworks a Project Targets:**
1. **Right-click project** -> **Properties** -> **Application**
2. **Edit "Target frameworks" field** (semicolon-separated list)
3. **Reload project** when prompted

## Prerequisites

### For .NET 8.0 Projects:
- **.NET 8 SDK** installed
- **Visual Studio 2022 17.8+** or **Visual Studio Code** with C# extension

### For .NET 9.0 Projects:
- **.NET 9 SDK** installed  
- **Visual Studio 2022 17.12+** or **Visual Studio Code** with C# extension

### For .NET 10.0 Projects:
- **.NET 10 SDK** installed
- **Visual Studio 2022 17.13+** or **Visual Studio Code** with C# extension

### For Windows-specific projects (WPF, Avalonia):
- **Windows 10 version 1809 or later**
- **Windows SDK** installed

### For MAUI projects:
- **MAUI workload** installed:
  ```bash
  dotnet workload install maui
  ```
- **Platform-specific requirements**:
  - **Android**: Android SDK 24.0+
  - **iOS**: Xcode (macOS only) 
  - **macOS**: Xcode (macOS only)
  - **Windows**: Windows 10 version 19041.0+

### Checking Your .NET Installation

To verify which .NET versions you have installed:

```bash
# List all installed .NET versions
dotnet --list-sdks

# Check current .NET version
dotnet --version

# List installed workloads
dotnet workload list
```

## Common Issues and Solutions

### Issue: "Can't see Start With Debugging Dropdown Button in toolbar"
**Solution**: 
- **Confirm the project is multi-targeted** (all projects in this solution are)
- **Set the project as startup project** (right-click -> "Set as Startup Project")
- **Look next to the Run without Debugging dropdown Button**

### Issue: "Can't see Target framework dropdown in Properties"
**Solution**: All projects in this solution are multi-targeted, so you'll see "Target frameworks" (plural) as an editable text field instead of a dropdown. You can edit the semicolon-separated list directly.

### Issue: "The specified framework is not available"
**Solution**: Install the required .NET SDK version:
```bash
# Download from: https://dotnet.microsoft.com/download
```

### Issue: Build errors with WebView components (Desktop Hybrid)
**Solution**: Ensure you have the correct Windows SDK and WebView2 runtime installed:
- Install Visual Studio with "Windows application development" workload
- WebView2 runtime is usually installed automatically with Windows 11 or can be downloaded from Microsoft

### Issue: MAUI project won't run
**Solution**: Install MAUI workload and ensure correct platform SDKs:
```bash
dotnet workload install maui
dotnet workload restore
```

### Issue: Android build fails (MAUI)
**Solution**: Ensure Android SDK is properly configured:
```bash
# Check Android SDK path in Visual Studio or set ANDROID_HOME environment variable
# Install Android SDK 24.0 or higher
```

### Issue: iOS/macOS build fails (MAUI - macOS only)
**Solution**: Ensure Xcode is installed and up to date:
```bash
# Install Xcode from Mac App Store
# Accept Xcode license: sudo xcodebuild -license accept
```

## Package Versioning

The project uses Central Package Management with conditional versioning:
- **For .NET 8.0**: Uses v8.x packages  
- **For .NET 9.0**: Uses v9.x packages
- **For .NET 10.0**: Uses v10.x packages

This is configured in the `Directory.Packages.props` file at the solution root.

### MAUI-Specific Packages:
- **Microsoft.Maui.Controls**: Automatically versioned based on target framework
- **Microsoft.AspNetCore.Components.WebView.Maui**: Matches .NET version
- **Microsoft.Extensions.Logging.Debug**: Matches .NET version

### Blazor-Specific Packages:
- **Microsoft.AspNetCore.Components.WebAssembly**: Framework-specific versions
- **Microsoft.AspNetCore.Components.Web**: Framework-specific versions

### Desktop Hybrid-Specific Packages:
- **Microsoft.AspNetCore.Components.WebView.Wpf**: Framework-specific versions for WPF
- **Custom Avalonia WebView**: Uses Baksteen.Avalonia.Blazor library for Avalonia integration

## Testing and Validation

### Testing Multi-Target Builds

To ensure all target frameworks build correctly:

```bash
# Build entire solution for all target frameworks
dotnet build

# Build specific project for all its target frameworks
dotnet build samples/HybridSamples/HybridSample.Wpf

# Build MAUI project for all platforms
dotnet build samples/Blazing.Mvvm.Sample.HybridMaui

# Restore packages for all frameworks
dotnet restore
```

### Running MAUI Apps on Different Platforms

#### Android Emulator:
```bash
# Build and deploy to Android
dotnet build samples/Blazing.Mvvm.Sample.HybridMaui -f net9.0-android
# Deploy to emulator or device via Visual Studio
```

#### Windows:
```bash
# Run on Windows
dotnet run --project samples/Blazing.Mvvm.Sample.HybridMaui -f net9.0-windows10.0.19041.0
```

#### iOS Simulator (macOS only):
```bash
# Build for iOS
dotnet build samples/Blazing.Mvvm.Sample.HybridMaui -f net9.0-ios
# Deploy via Visual Studio for Mac or Xcode
```