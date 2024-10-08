﻿using System.Collections.Generic;
using System.Threading.Tasks;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI.Tests.Stubs;

public class ProductRepositoryStub : IRepository<Product>
{
    public bool CanWrite { get; } = true;
    
    public async Task<Product?> GetByArticleAsync(string article)
    {
        return null;
    }
    
    public async Task<List<Product>?> GetAllAsync()
    {
        return null;
    }
    
    public async Task AddAsync(Product product)
    {
        return;
    }
    
    public async Task UpdateAsync(Product product)
    {
        return;
    }
    
    public async Task DeleteAsync(string? article)
    {
        return;
    }
    
    public Task<bool> ValidateAccessKeyAsync(string accessKey)
    {
        throw new System.NotImplementedException();
    }
}