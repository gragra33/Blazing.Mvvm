// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class AsyncRelayCommandPageViewModel : SamplePageViewModel
{
    public AsyncRelayCommandPageViewModel(IFilesService filesService) 
        : base(filesService)
    {
        DownloadTextCommand = new AsyncRelayCommand(DownloadTextAsync);
    }

    public IAsyncRelayCommand DownloadTextCommand { get; }

    private async Task<string> DownloadTextAsync()
    {
        await Task.Delay(3000); // Simulate a web request

        return "Hello world!";
    }
}
