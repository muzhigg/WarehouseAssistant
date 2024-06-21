using Microsoft.EntityFrameworkCore;
using WarehouseAssistant.Data.DbContexts;
using WarehouseAssistant.Data.Models;

namespace WarehouseAssistant.Data.Repositories
{
    internal class MarketingMaterialRepository : IRepository<MarketingMaterial>
    {
        private readonly WarehouseDbContext _context;

        public MarketingMaterialRepository(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<MarketingMaterial> GetByArticleAsync(string article)
        {
            return await _context.MarketingMaterials.FindAsync(article);
        }

        public async Task<IEnumerable<MarketingMaterial>> GetAllAsync()
        {
            return await _context.MarketingMaterials.ToListAsync();
        }

        public async Task AddAsync(MarketingMaterial marketingMaterial)
        {
            await _context.MarketingMaterials.AddAsync(marketingMaterial);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MarketingMaterial marketingMaterial)
        {
            _context.MarketingMaterials.Update(marketingMaterial);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string article)
        {
            var marketingMaterial = await _context.MarketingMaterials.FindAsync(article);
            if (marketingMaterial != null)
            {
                _context.MarketingMaterials.Remove(marketingMaterial);
                await _context.SaveChangesAsync();
            }
        }
    }
}
