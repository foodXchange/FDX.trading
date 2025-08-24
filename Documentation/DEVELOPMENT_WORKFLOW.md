# FoodX Development Workflow Guide

## Architecture Overview

The FoodX platform uses a **Domain-Driven Design** approach with **vertical slices** for feature development. This guide explains how to develop features using our repository pattern and Unit of Work implementation.

## Database Architecture

- **Single Database**: `FoodXBusinessDB`
- **Logical Separation**: Using schemas (Core, Trading, Analytics, Portal)
- **Shared Identity**: AspNetUsers table for all portals

## Development Approach

### 1. Vertical Slice Development

Instead of developing all entities at once, focus on complete features that span multiple layers:

```
Feature: "Add Product to Catalog"
├── API Endpoint (FoodX.Admin)
├── Business Logic (FoodX.Core)
├── Data Access (Repository/UnitOfWork)
├── UI Component (FoodX.SharedUI)
└── Database (Products table)
```

### 2. Using the Repository Pattern

#### Basic CRUD Operations

```csharp
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Get all products
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _unitOfWork.Products.GetActiveProductsAsync();
    }

    // Add new product
    public async Task<Product> AddProductAsync(Product product)
    {
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return product;
    }

    // Update product
    public async Task UpdateProductAsync(Product product)
    {
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

#### Complex Operations with Transactions

```csharp
public async Task<Order> CreateOrderWithItemsAsync(Order order, List<OrderItem> items)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Add order
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        // Add order items
        foreach (var item in items)
        {
            item.OrderId = order.Id;
            await _unitOfWork.OrderItems.AddAsync(item);
        }
        
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();
        
        return order;
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### 3. Feature Development Steps

#### Step 1: Define the Entity (if new)

```csharp
// FoodX.Core/Models/Entities/YourEntity.cs
public class YourEntity : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    // Other properties
}
```

#### Step 2: Add to DbContext

```csharp
// FoodX.Core/Data/FoodXBusinessContext.cs
public DbSet<YourEntity> YourEntities { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<YourEntity>(entity =>
    {
        entity.ToTable("YourEntities", "Core"); // Use appropriate schema
        entity.HasKey(e => e.Id);
        // Other configurations
    });
}
```

#### Step 3: Create Repository (if needed)

For simple entities, use the generic repository:
```csharp
// In UnitOfWork
public IGenericRepository<YourEntity> YourEntities => 
    _yourEntities ??= new GenericRepository<YourEntity>(_context);
```

For complex entities, create specific repository:
```csharp
// FoodX.Core/Repositories/IYourEntityRepository.cs
public interface IYourEntityRepository : IGenericRepository<YourEntity>
{
    Task<IEnumerable<YourEntity>> GetSpecializedDataAsync();
}

// FoodX.Core/Repositories/YourEntityRepository.cs
public class YourEntityRepository : GenericRepository<YourEntity>, IYourEntityRepository
{
    public async Task<IEnumerable<YourEntity>> GetSpecializedDataAsync()
    {
        // Custom implementation
    }
}
```

#### Step 4: Create Service Layer

```csharp
// FoodX.Core/Services/YourEntityService.cs
public class YourEntityService : IYourEntityService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public YourEntityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    // Implement business logic
}
```

#### Step 5: Add API Endpoints

```csharp
// FoodX.Admin/Controllers/YourEntityController.cs
[ApiController]
[Route("api/[controller]")]
public class YourEntityController : ControllerBase
{
    private readonly IYourEntityService _service;
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _service.GetAllAsync();
        return Ok(entities);
    }
}
```

#### Step 6: Create UI Components

```razor
@* FoodX.SharedUI/Components/YourEntityList.razor *@
@inherits ComponentBase

<MudTable Items="@Entities" Hover="true">
    <HeaderContent>
        <MudTh>Name</MudTh>
        <MudTh>Description</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Name</MudTd>
        <MudTd>@context.Description</MudTd>
    </RowTemplate>
</MudTable>

@code {
    [Parameter] public List<YourEntity> Entities { get; set; }
}
```

## Development Priorities

### Phase 1: Core Trading Features
1. **Product Management**
   - Product CRUD operations
   - Category management
   - SKU validation
   - Pricing tiers

2. **Order Processing**
   - Order creation
   - Order status workflow
   - Order history

3. **Quote System**
   - RFQ submission
   - Quote generation
   - Quote comparison

### Phase 2: User Management
1. **Company Profiles**
   - Company registration
   - Profile management
   - Document uploads

2. **User Roles**
   - Role assignment
   - Permission management
   - Multi-portal access

### Phase 3: Advanced Features
1. **Exhibition Management**
   - Exhibition creation
   - Exhibitor registration
   - Booth assignment

2. **Analytics**
   - Sales reports
   - Product analytics
   - User activity tracking

## Testing Strategy

### Unit Tests
```csharp
[TestClass]
public class ProductServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private ProductService _service;
    
    [TestInitialize]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _service = new ProductService(_mockUnitOfWork.Object);
    }
    
    [TestMethod]
    public async Task AddProduct_Should_SaveToDatabase()
    {
        // Arrange
        var product = new Product { Name = "Test Product" };
        
        // Act
        await _service.AddProductAsync(product);
        
        // Assert
        _mockUnitOfWork.Verify(x => x.Products.AddAsync(product), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
```

### Integration Tests
```csharp
[TestClass]
public class ProductIntegrationTests
{
    private FoodXBusinessContext _context;
    private IUnitOfWork _unitOfWork;
    
    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<FoodXBusinessContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
            
        _context = new FoodXBusinessContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }
    
    [TestMethod]
    public async Task CompleteOrderWorkflow_Should_Work()
    {
        // Test complete feature workflow
    }
}
```

## Dependency Injection Setup

```csharp
// Program.cs in each portal
builder.Services.AddDbContext<FoodXBusinessContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
// Add other services
```

## Common Patterns

### 1. Pagination
```csharp
public async Task<PagedResult<Product>> GetProductsPagedAsync(int page, int pageSize)
{
    var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(
        pageNumber: page,
        pageSize: pageSize,
        filter: p => p.IsActive,
        orderBy: q => q.OrderBy(p => p.Name)
    );
    
    return new PagedResult<Product>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

### 2. Filtering and Searching
```csharp
public async Task<IEnumerable<Product>> SearchProductsAsync(ProductSearchCriteria criteria)
{
    var products = await _unitOfWork.Products.FindAsync(p =>
        (string.IsNullOrEmpty(criteria.Name) || p.Name.Contains(criteria.Name)) &&
        (!criteria.MinPrice.HasValue || p.Price >= criteria.MinPrice) &&
        (!criteria.MaxPrice.HasValue || p.Price <= criteria.MaxPrice) &&
        (string.IsNullOrEmpty(criteria.Category) || p.Category == criteria.Category)
    );
    
    return products;
}
```

### 3. Bulk Operations
```csharp
public async Task ImportProductsAsync(List<Product> products)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Validate all products first
        foreach (var product in products)
        {
            if (!await _unitOfWork.Products.IsSkuUniqueAsync(product.SKU))
            {
                throw new InvalidOperationException($"Duplicate SKU: {product.SKU}");
            }
        }
        
        // Bulk insert
        await _unitOfWork.Products.AddRangeAsync(products);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

## Best Practices

1. **Always use UnitOfWork for transactions**
   - Ensures data consistency
   - Proper rollback on errors

2. **Keep business logic in services**
   - Controllers should be thin
   - Services contain business rules

3. **Use DTOs for API responses**
   - Don't expose entities directly
   - Control what data is sent to clients

4. **Implement proper error handling**
   - Use try-catch in services
   - Return meaningful error messages

5. **Follow naming conventions**
   - Tables: Use schema prefix (Trading.Orders)
   - Services: EntityNameService
   - Repositories: EntityNameRepository

## Development Tools

### Visual Studio Configuration
1. Open `C:\Users\fdxadmin\source\repos\FDX.trading\FDX.trading.sln`
2. Set startup project (Admin, Buyer, Supplier, or Marketplace)
3. Press F5 to run

### Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName -c FoodXBusinessContext -p FoodX.Core -s FoodX.Admin

# Update database
dotnet ef database update -c FoodXBusinessContext -p FoodX.Core -s FoodX.Admin
```

### Running Multiple Portals
```bash
# Use the startup script
.\start-all-portals.ps1
```

## Troubleshooting

### Common Issues

1. **Connection String Issues**
   - Check appsettings.json
   - Verify SQL Server is running
   - Check firewall settings

2. **Migration Errors**
   - Ensure FoodX.Core is built
   - Check for pending migrations
   - Verify database permissions

3. **Dependency Injection Errors**
   - Register all services in Program.cs
   - Check interface implementations
   - Verify scoped vs singleton lifetime

## Next Steps

1. Start with a simple feature (e.g., Product CRUD)
2. Implement using the patterns shown above
3. Test thoroughly
4. Move to next feature in priority list
5. Refactor and optimize as needed

## Support

For questions or issues:
- Check existing code examples in the repository
- Review test implementations
- Consult architecture documentation