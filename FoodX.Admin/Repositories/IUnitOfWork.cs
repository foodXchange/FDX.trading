using FoodX.Admin.Models;

namespace FoodX.Admin.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Company> Companies { get; }
    IRepository<Product> Products { get; }
    IRepository<Buyer> Buyers { get; }
    IRepository<Supplier> Suppliers { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}