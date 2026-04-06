// This file is used to configure code analysis warnings in the CodeFixProviders directory.
// Code fix providers are allowed to reference Microsoft.CodeAnalysis.Workspaces, while analyzers are not.

using System.Diagnostics.CodeAnalysis;

// Suppress RS1038 for the entire CodeFixProviders directory
// Code fix providers need access to Workspaces APIs for document editing
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1038:This compiler extension should not be implemented in an assembly containing a reference to Microsoft.CodeAnalysis.Workspaces", Justification = "Code fix providers require Workspaces APIs", Scope = "namespaceanddescendants", Target = "~N:Blazing.Mvvm.Analyzers.CodeFixProviders")]
