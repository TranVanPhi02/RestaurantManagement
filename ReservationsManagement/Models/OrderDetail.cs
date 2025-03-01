using System;
using System.Collections.Generic;

namespace ReservationsManagement.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }
    public int OrderId { get; set; }
    public int MenuId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; } // Không NULL
    public virtual Order Order { get; set; }
    public virtual Menu Menu { get; set; }
}
