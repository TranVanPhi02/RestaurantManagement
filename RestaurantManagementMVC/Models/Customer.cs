using System;
using System.Collections.Generic;

namespace RestaurantManagementMVC.Models;

public partial class Customer
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Preferences { get; set; }

    public int? LoyaltyPoints { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User User { get; set; } = null!;
}
