using System;
using System.Collections.Generic;

namespace ReservationsManagement.Models;

public partial class Restaurant
{
    public int ResId { get; set; }

    public int? CategoryId { get; set; }

    public string NameOwner { get; set; } = null!;

    public string NameRes { get; set; } = null!;

    public string? Image { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    public string Address { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Description { get; set; }

    public string? Subdescription { get; set; }

    public string Location { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public decimal? Price { get; set; }

    public string? Discount { get; set; }

    public string? Status { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<CategoryMenu> CategoryMenus { get; set; } = new List<CategoryMenu>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
