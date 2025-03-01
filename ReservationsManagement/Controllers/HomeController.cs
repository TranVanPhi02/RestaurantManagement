using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReservationsManagement.Models;

namespace ReservationsManagement.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Home", "Customer");
    }
}
