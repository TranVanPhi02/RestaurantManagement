using System;
using System.Collections.Generic;

namespace RestaurantManagementMVC.Models;

public partial class Table
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public int TableNumber { get; set; }

    public int Capacity { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Restaurant Restaurant { get; set; } = null!;
}
