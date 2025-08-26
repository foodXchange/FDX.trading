using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoodX.Core.Data
{
    /// <summary>
    /// Base DbContext for all FoodX database contexts
    /// Provides common configuration and functionality
    /// </summary>
    public abstract class BaseDbContext : DbContext
    {
        private IDbContextTransaction? _currentTransaction;

        protected BaseDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Configure common model settings for all entities
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply common configurations
            ApplyCommonConfigurations(modelBuilder);

            // Apply entity-specific configurations
            ApplyEntityConfigurations(modelBuilder);
        }

        /// <summary>
        /// Apply common configurations to all entities
        /// </summary>
        protected virtual void ApplyCommonConfigurations(ModelBuilder modelBuilder)
        {
            // Set default schema if needed
            // modelBuilder.HasDefaultSchema("dbo");

            // Apply global query filters (e.g., soft delete)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Check if entity has IsDeleted property
                var isDeletedProperty = entityType.FindProperty("IsDeleted");
                if (isDeletedProperty != null && isDeletedProperty.ClrType == typeof(bool))
                {
                    // Create parameter for entity type
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");

                    // Create expression: e => !e.IsDeleted
                    var propertyAccess = System.Linq.Expressions.Expression.Property(parameter, isDeletedProperty.PropertyInfo!);
                    var notDeleted = System.Linq.Expressions.Expression.Not(propertyAccess);
                    var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);

                    // Apply global filter
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }

                // Set default values for common properties
                var createdAtProperty = entityType.FindProperty("CreatedAt");
                if (createdAtProperty != null && createdAtProperty.ClrType == typeof(DateTime))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property("CreatedAt")
                        .HasDefaultValueSql("GETUTCDATE()");
                }

                var updatedAtProperty = entityType.FindProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.ClrType == typeof(DateTime))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property("UpdatedAt")
                        .HasDefaultValueSql("GETUTCDATE()");
                }
            }
        }

        /// <summary>
        /// Override in derived classes to apply entity-specific configurations
        /// </summary>
        protected abstract void ApplyEntityConfigurations(ModelBuilder modelBuilder);

        /// <summary>
        /// Override SaveChangesAsync to automatically update timestamps
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Override SaveChanges to automatically update timestamps
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Update timestamps for modified entities
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var entity = entry.Entity;
                var entityType = entity.GetType();

                // Update CreatedAt for new entities
                if (entry.State == EntityState.Added)
                {
                    var createdAtProperty = entityType.GetProperty("CreatedAt");
                    if (createdAtProperty != null && createdAtProperty.PropertyType == typeof(DateTime))
                    {
                        createdAtProperty.SetValue(entity, DateTime.UtcNow);
                    }
                }

                // Update UpdatedAt for modified entities
                var updatedAtProperty = entityType.GetProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime))
                {
                    updatedAtProperty.SetValue(entity, DateTime.UtcNow);
                }

                // Update ModifiedBy if user context is available
                var modifiedByProperty = entityType.GetProperty("ModifiedBy");
                if (modifiedByProperty != null && modifiedByProperty.PropertyType == typeof(string))
                {
                    // This would need to be injected via DI in real implementation
                    // modifiedByProperty.SetValue(entity, GetCurrentUserId());
                }
            }
        }

        #region Transaction Management

        /// <summary>
        /// Begin a new transaction
        /// </summary>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress");
            }

            _currentTransaction = await Database.BeginTransactionAsync();
            return _currentTransaction;
        }

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                await _currentTransaction?.CommitAsync()!;
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                DisposeTransaction();
            }
        }

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _currentTransaction?.RollbackAsync()!;
            }
            finally
            {
                DisposeTransaction();
            }
        }

        private void DisposeTransaction()
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Bulk insert entities
        /// </summary>
        public virtual async Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class
        {
            await Set<T>().AddRangeAsync(entities);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Bulk update entities
        /// </summary>
        public virtual async Task BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class
        {
            Set<T>().UpdateRange(entities);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Bulk delete entities
        /// </summary>
        public virtual async Task BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class
        {
            Set<T>().RemoveRange(entities);
            await SaveChangesAsync();
        }

        #endregion

        /// <summary>
        /// Execute raw SQL query
        /// </summary>
        public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            return await Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}