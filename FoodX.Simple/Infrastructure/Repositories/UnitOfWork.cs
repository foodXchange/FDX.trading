using Microsoft.EntityFrameworkCore.Storage;
using FoodX.Simple.Data;
using FoodX.Simple.Domain.Interfaces;

namespace FoodX.Simple.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private IProductBriefRepository? _productBriefs;
        private IProductRepository? _products;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IProductBriefRepository ProductBriefs =>
            _productBriefs ??= new ProductBriefRepository(_context);

        public IProductRepository Products =>
            _products ??= new ProductRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }

    public class ProductBriefRepository : Repository<Models.ProductBrief>, IProductBriefRepository
    {
        public ProductBriefRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Models.ProductBrief>> GetByStatusAsync(string status)
        {
            return await FindAsync(pb => pb.Status == status);
        }

        public async Task<IEnumerable<Models.ProductBrief>> GetByUserAsync(string userId)
        {
            return await FindAsync(pb => pb.CreatedBy == userId);
        }
    }

    public class ProductRepository : Repository<Models.Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Models.Product>> GetByCategoryAsync(string category)
        {
            return await FindAsync(p => p.Category == category);
        }

        public async Task<IEnumerable<Models.Product>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await FindAsync(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }
    }
}