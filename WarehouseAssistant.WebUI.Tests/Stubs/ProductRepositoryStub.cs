using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;

namespace WarehouseAssistant.WebUI.Tests.Stubs;

public class ProductRepositoryStub : IRepository<Product>
{
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
}