using System;
using System.Collections.Generic;

namespace RestaurantManagementMVC.Models;

public partial class Restaurant
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Description { get; set; }

    public string? OpeningHours { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();

    public virtual User User { get; set; } = null!;
}
