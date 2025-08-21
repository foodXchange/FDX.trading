using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FoodX.Admin.Data;
using FoodX.Admin.Models;

namespace FoodX.Admin.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FoodXDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    private IRepository<Company>? _companies;
    private IRepository<Product>? _products;
    private IRepository<Buyer>? _buyers;
    private IRepository<Supplier>? _suppliers;

    public UnitOfWork(FoodXDbContext context, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public IRepository<Company> Companies => 
        _companies ??= new Repository<Company>(_context, _loggerFactory.CreateLogger<Repository<Company>>());

    public IRepository<Product> Products => 
        _products ??= new Repository<Product>(_context, _loggerFactory.CreateLogger<Repository<Product>>());

    public IRepository<Buyer> Buyers => 
        _buyers ??= new Repository<Buyer>(_context, _loggerFactory.CreateLogger<Repository<Buyer>>());

    public IRepository<Supplier> Suppliers => 
        _suppliers ??= new Repository<Supplier>(_context, _loggerFactory.CreateLogger<Repository<Supplier>>());

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict occurred");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error occurred");
            throw;
        }
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            _logger.LogWarning("Transaction already in progress");
            return;
        }

        _transaction = await _context.Database.BeginTransactionAsync();
        _logger.LogDebug("Database transaction started");
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            _logger.LogWarning("No transaction to commit");
            return;
        }

        try
        {
            await _transaction.CommitAsync();
            _logger.LogDebug("Database transaction committed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            _logger.LogWarning("No transaction to rollback");
            return;
        }

        try
        {
            await _transaction.RollbackAsync();
            _logger.LogDebug("Database transaction rolled back");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction");
            throw;
        }
        finally
        {
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