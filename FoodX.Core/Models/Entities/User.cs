using System;
using System.Collections.Generic;
using FoodX.Core.Models.Base;

namespace FoodX.Core.Models.Entities
{
    public class User : BaseEntity
    {
        public string AspNetUserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string TimeZone { get; set; } = "UTC";
        public string PreferredLanguage { get; set; } = "en";

        // Navigation properties
        public virtual Company? Company { get; set; }
        public virtual ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
        public virtual ICollection<Quote> CreatedQuotes { get; set; } = new List<Quote>();
    }
}