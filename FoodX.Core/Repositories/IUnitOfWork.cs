using System;
using System.Threading.Tasks;
using FoodX.Core.Models.Entities;
using FoodX.Core.Models;

namespace FoodX.Core.Repositories
{
    /// <summary>
    /// Unit of Work interface for managing transactions and repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repository properties for common entities
        IProductRepository Products { get; }
        IGenericRepository<Company> Companies { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderItem> OrderItems { get; }
        IGenericRepository<Buyer> Buyers { get; }
        IGenericRepository<Supplier> Suppliers { get; }
        IGenericRepository<Quote> Quotes { get; }
        IGenericRepository<QuoteItem> QuoteItems { get; }
        IGenericRepository<RFQ> RFQs { get; }
        IGenericRepository<RFQItem> RFQItems { get; }
        IGenericRepository<Exhibition> Exhibitions { get; }
        IGenericRepository<Exhibitor> Exhibitors { get; }
        IGenericRepository<Project> Projects { get; }

        // Transaction methods
        Task<int> SaveChangesAsync();
        int SaveChanges();
        Task<int> CompleteAsync(); // Alias for SaveChangesAsync
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Generic repository access
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    }
}