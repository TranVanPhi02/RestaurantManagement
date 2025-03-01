using System;
using System.Collections.Generic;

namespace RestaurantManagementMVC.Models;

public partial class Order
{
    public int Id { get; set; }

    public int DinerId { get; set; }

    public int TableId { get; set; }

    public DateTime OrderDate { get; set; }

    public int GuestsCount { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer Diner { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Table Table { get; set; } = null!;
}
