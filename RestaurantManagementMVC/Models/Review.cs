using System;
using System.Collections.Generic;

namespace RestaurantManagementMVC.Models;

public partial class Review
{
    public int Id { get; set; }

    public int DinerId { get; set; }

    public int RestaurantId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer Diner { get; set; } = null!;

    public virtual Restaurant Restaurant { get; set; } = null!;
}
