using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WarehouseAssistant.Data.DbContexts;
using WarehouseAssistant.Data.Models;

namespace WarehouseAssistant.Data.Repositories
{
    internal class ProductRepository : IRepository<Product>
    {
        private readonly WarehouseDbContext _context;

        public ProductRepository(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetByArticleAsync(string article)
        {
            return await _context.Products.FindAsync(article);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string article)
        {
            var product = await _context.Products.FindAsync(article);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
