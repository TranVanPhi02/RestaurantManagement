using System;
using System.Collections.Generic;

namespace RestaurantManagementMVC.Models;

public partial class MenuItem
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public string? Category { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Restaurant Restaurant { get; set; } = null!;
}
