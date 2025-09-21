namespace FoodX.Simple.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductBriefRepository ProductBriefs { get; }
        IProductRepository Products { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

    public interface IProductBriefRepository : IRepository<Models.ProductBrief>
    {
        Task<IEnumerable<Models.ProductBrief>> GetByStatusAsync(string status);
        Task<IEnumerable<Models.ProductBrief>> GetByUserAsync(string userId);
    }

    public interface IProductRepository : IRepository<Models.Product>
    {
        Task<IEnumerable<Models.Product>> GetByCategoryAsync(string category);
        Task<IEnumerable<Models.Product>> SearchAsync(string searchTerm);
    }
}