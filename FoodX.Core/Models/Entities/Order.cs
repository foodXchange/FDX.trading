using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodX.Core.Models.Base;

namespace FoodX.Core.Models.Entities;

public class Order : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";
    
    // Foreign keys
    public int BuyerId { get; set; }
    public int SupplierId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TaxAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ShippingCost { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountAmount { get; set; }
    
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    [MaxLength(100)]
    public string? PaymentTerms { get; set; }
    
    [MaxLength(100)]
    public string? PaymentStatus { get; set; }
    
    public DateTime? PaymentDueDate { get; set; }
    
    [MaxLength(100)]
    public string? DeliveryTerms { get; set; }
    
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    public DateTime? ActualDeliveryDate { get; set; }
    
    public string? ShippingAddress { get; set; }
    
    public string? BillingAddress { get; set; }
    
    [MaxLength(100)]
    public string? PONumber { get; set; }
    
    public string? Notes { get; set; }
    
    public string? InternalNotes { get; set; }
    
    // Navigation properties
    [ForeignKey("BuyerId")]
    public virtual Buyer Buyer { get; set; } = null!;
    
    [ForeignKey("SupplierId")]
    public virtual Supplier Supplier { get; set; } = null!;
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}