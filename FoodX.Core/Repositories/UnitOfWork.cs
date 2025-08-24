using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FoodX.Core.Data;
using FoodX.Core.Models.Entities;
using FoodX.Core.Models;

namespace FoodX.Core.Repositories
{
    /// <summary>
    /// Unit of Work implementation for managing transactions and repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FoodXBusinessContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;
        
        // Repository instances
        private IProductRepository? _products;
        private IGenericRepository<Company>? _companies;
        private IGenericRepository<Order>? _orders;
        private IGenericRepository<OrderItem>? _orderItems;
        private IGenericRepository<Buyer>? _buyers;
        private IGenericRepository<Supplier>? _suppliers;
        private IGenericRepository<Quote>? _quotes;
        private IGenericRepository<QuoteItem>? _quoteItems;
        private IGenericRepository<RFQ>? _rfqs;
        private IGenericRepository<RFQItem>? _rfqItems;
        private IGenericRepository<Exhibition>? _exhibitions;
        private IGenericRepository<Exhibitor>? _exhibitors;
        private IGenericRepository<Project>? _projects;
        
        // Dictionary to store generic repositories
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(FoodXBusinessContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Repository Properties

        public IProductRepository Products => _products ??= new ProductRepository(_context);
        public IGenericRepository<Company> Companies => _companies ??= new GenericRepository<Company>(_context);
        public IGenericRepository<Order> Orders => _orders ??= new GenericRepository<Order>(_context);
        public IGenericRepository<OrderItem> OrderItems => _orderItems ??= new GenericRepository<OrderItem>(_context);
        public IGenericRepository<Buyer> Buyers => _buyers ??= new GenericRepository<Buyer>(_context);
        public IGenericRepository<Supplier> Suppliers => _suppliers ??= new GenericRepository<Supplier>(_context);
        public IGenericRepository<Quote> Quotes => _quotes ??= new GenericRepository<Quote>(_context);
        public IGenericRepository<QuoteItem> QuoteItems => _quoteItems ??= new GenericRepository<QuoteItem>(_context);
        public IGenericRepository<RFQ> RFQs => _rfqs ??= new GenericRepository<RFQ>(_context);
        public IGenericRepository<RFQItem> RFQItems => _rfqItems ??= new GenericRepository<RFQItem>(_context);
        public IGenericRepository<Exhibition> Exhibitions => _exhibitions ??= new GenericRepository<Exhibition>(_context);
        public IGenericRepository<Exhibitor> Exhibitors => _exhibitors ??= new GenericRepository<Exhibitor>(_context);
        public IGenericRepository<Project> Projects => _projects ??= new GenericRepository<Project>(_context);

        #endregion

        #region Generic Repository Access

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new GenericRepository<TEntity>(_context);
            }
            
            return (IGenericRepository<TEntity>)_repositories[type];
        }

        #endregion

        #region Save Methods

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts
                throw new InvalidOperationException("A concurrency conflict occurred. The entity may have been modified or deleted.", ex);
            }
            catch (DbUpdateException ex)
            {
                // Handle database update exceptions
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }

        public int SaveChanges()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("A concurrency conflict occurred. The entity may have been modified or deleted.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }

        public async Task<int> CompleteAsync()
        {
            return await SaveChangesAsync();
        }

        #endregion

        #region Transaction Methods

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }
            
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }
            
            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
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
                throw new InvalidOperationException("No transaction is in progress.");
            }
            
            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        #endregion

        #region IDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}