﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace WarehouseAssistant.WebUI.Tests.Stubs;

public class LocalStorageStub : ILocalStorageService
{
    public async ValueTask ClearAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return;
    }

    public async ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = new CancellationToken())
    {
        return default;
    }

    public async ValueTask<string?> GetItemAsStringAsync(string key, CancellationToken cancellationToken = new CancellationToken())
    {
        return null;
    }

    public async ValueTask<string?> KeyAsync(int index, CancellationToken cancellationToken = new CancellationToken())
    {
        return null;
    }

    public async ValueTask<IEnumerable<string>> KeysAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask<int> LengthAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public event EventHandler<ChangingEventArgs>? Changing;
    public event EventHandler<ChangedEventArgs>? Changed;
}