using System;
using System.Collections.Generic;

namespace ReservationsManagement.Models;

public partial class Menu
{
    public int MenuId { get; set; }

    public int RestaurantId { get; set; }

    public int? CategoryMenuId { get; set; }

    public string DishName { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Status { get; set; }

    public string? Image { get; set; }

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public virtual CategoryMenu? CategoryMenu { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Restaurant Restaurant { get; set; } = null!;
}
