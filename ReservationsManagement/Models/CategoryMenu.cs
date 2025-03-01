using System;
using System.Collections.Generic;

namespace ReservationsManagement.Models;

public partial class CategoryMenu
{
    public int CategoryMenuId { get; set; }

    public int RestaurantId { get; set; }

    public string? CategoryMenuName { get; set; }

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    public virtual Restaurant Restaurant { get; set; } = null!;
}
