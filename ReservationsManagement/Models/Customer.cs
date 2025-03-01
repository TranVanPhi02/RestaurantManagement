using System;
using System.Collections.Generic;

namespace ReservationsManagement.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Image { get; set; }

    public string Email { get; set; } = null!;

    public DateTime? Dob { get; set; }

    public string? Gender { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Status { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
