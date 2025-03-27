using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ReservationsManagement.Models;
using System.Net;

namespace ReservationsManagement.Controllers.restaurant
{
    [Route("Restaurant")]
    public class RestaurantController : Controller
    {
        private readonly ReservationsManagementContext _context;
        private readonly IWebHostEnvironment _environment;
        public RestaurantController(ReservationsManagementContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public bool checkSession()
        {
            bool checkS = true;
            var httpContext = HttpContext;
            if (httpContext != null && httpContext.Session != null)
            {
                string isCustomerAuthenticated = httpContext.Session.GetString("restaurant");
                if (string.IsNullOrEmpty(isCustomerAuthenticated))
                {
                    checkS = false;
                }
            }
            return checkS;
        }
        [HttpGet("dashboard")]
        public IActionResult Dashboard(string service, int year)
        {
            if (checkSession() == false)
            {
                return RedirectToAction("Login");
            }
            // Task Bar
            ViewBag.TotalOrder = _context.Orders.Count();
            ViewBag.TotalCustomer = _context.Customers.Count();
          
         
            ViewBag.TotalContact = _context.Contacts.Count();

            // ORDER STATUS
            ViewBag.Wait = _context.Orders.Where(x => x.StatusOrder.Equals("Wait")).Count();
            ViewBag.Accept = _context.Orders.Where(x => x.StatusOrder.Equals("Accept")).Count();
            ViewBag.Process = _context.Orders.Where(x => x.StatusOrder.Equals("Process")).Count();
            ViewBag.Done = _context.Orders.Where(x => x.StatusOrder.Equals("Done")).Count();
            ViewBag.Cancel = _context.Orders.Where(x => x.StatusOrder.Equals("Cancel")).Count();


         
            // SEARCH TOTAL Order YEAR
            if (service == null)
            {
                service = "TotalOrderCurrentYear";
            }

            int yearinchart = 0;

            if (service.Equals("TotalOrderCurrentYear"))
            {
                ViewBag.Year = DateTime.Now.Year;
                yearinchart = DateTime.Now.Year;
                ViewBag.TotalOrderYear = _context.Orders
                .Where(x => x.CreateDate.Value.Year == DateTime.Now.Year)
                .Count();
            }
            if (service.Equals("searchOrderYear"))
            {
                ViewBag.Year = year;
                yearinchart = year;
                ViewBag.TotalOrderYear = _context.Orders
                .Where(x => x.CreateDate.Value.Year == year)
                .Count();
            }

          


      
            return View();
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            var session = HttpContext.Session;

            if (session.Keys.Contains("restaurant"))
            {
                session.Remove("restaurant");
            }
            return RedirectToAction("Login");
        }

        [HttpGet("authenticate/login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("authenticate/login")]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMess = "Tên đăng nhập hoặc mật khẩu không được bỏ trống.";
                return View();
            }

            // Kiểm tra thông tin tài khoản
            var restaurant = _context.Restaurants
                                      .FirstOrDefault(x => x.Username.Equals(username) && x.Password.Equals(password));

            if (restaurant != null)
            {
                // Kiểm tra trạng thái tài khoản
                if (restaurant.Status.Equals("Deactive"))
                {
                    ViewBag.ErrorMess = "Tài khoản đang bị vô hiệu hóa.";
                    return View();
                }

                // Lưu thông tin vào session
                string restaurantJson = JsonConvert.SerializeObject(restaurant);
                HttpContext.Session.SetString("restaurant", restaurantJson);

                // Chuyển hướng đến Dashboard
                return RedirectToAction("Dashboard");
            }
            else
            {
                // Nếu không tìm thấy tài khoản
                ViewBag.ErrorMess = "Tên đăng nhập hoặc mật khẩu sai.";
                return View();
            }
        }
        [HttpGet("orderlist")]
        public IActionResult OrderList()
        {
            if (!checkSession())
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Lấy danh sách tất cả đơn hàng, sắp xếp theo ngày tạo mới nhất
                var orders = _context.Orders
                                     .Include(o => o.Customer)
                                     .OrderByDescending(o => o.CreateDate)
                                     .ToList();

                ViewBag.OrderList = orders ?? new List<Order>();

                if (!orders.Any())
                {
                    ViewBag.ErrorMessage = "There are no orders available.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.OrderList = new List<Order>();
                ViewBag.ErrorMessage = "An error occurred while fetching orders. Please try again.";
                Console.WriteLine($"Error: {ex.Message}");
            }

            return View();
        }
        [HttpPost("change-order-status")]
        public IActionResult ChangeOrderStatus(int orderId)
        {
            if (!checkSession())
            {
                return RedirectToAction("Login");
            }

            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("OrderList");
                }

                if (order.StatusOrder != "Pending")
                {
                    TempData["ErrorMessage"] = "Order status cannot be changed.";
                    return RedirectToAction("OrderList");
                }

                order.StatusOrder = "Accept";
                _context.Orders.Update(order);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Order status updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating order status.";
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction("OrderList");
        }

        [HttpGet("orderdetail/{orderId}")]
        public IActionResult OrderDetail(int orderId)
        {
            if (!checkSession())
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Lấy đơn hàng theo ID và bao gồm thông tin chi tiết đơn hàng
                var order = _context.Orders
                                    .Include(o => o.Customer)
                                    .Include(o => o.OrderDetails)
                                    .ThenInclude(od => od.Menu) // Bao gồm thông tin món ăn
                                    .FirstOrDefault(o => o.OrderId == orderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("OrderList");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while fetching order details.";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("OrderList");
            }
        }
        [HttpGet("customerlist")]
        public IActionResult CustomerList()
        {
            if (!checkSession())
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Lấy danh sách tất cả khách hàng từ cơ sở dữ liệu
                var customers = _context.Customers.ToList();

                ViewBag.CustomerList = customers ?? new List<Customer>();
                ViewBag.CustomerList = customers ?? new List<Customer>();

                if (!customers.Any())
                {
                    ViewBag.ErrorMessage = "No customers found.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.CustomerList = new List<Customer>();
                ViewBag.ErrorMessage = "An error occurred while fetching customers.";
                Console.WriteLine($"Error: {ex.Message}");
            }

            return View();
        }
        // Để deactivate tài khoản
        [HttpPost("deactivate-customer")]
        public IActionResult DeactivateCustomer(int customerId)
        {
            if (!checkSession())
            {
                return RedirectToAction("Login");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("CustomerList");
            }

            // Đổi trạng thái tài khoản thành "Deactive"
            customer.Status = "Deactive";
            _context.Customers.Update(customer);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Customer account deactivated successfully!";
            return RedirectToAction("CustomerList");
        }

        // Để activate tài khoản
        [HttpPost("activate-customer")]
        public IActionResult ActivateCustomer(int customerId)
        {
            if (!checkSession())
            {
                return RedirectToAction("Login");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("CustomerList");
            }

            // Đổi trạng thái tài khoản thành "Active"
            customer.Status = "Active";
            _context.Customers.Update(customer);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Customer account activated successfully!";
            return RedirectToAction("CustomerList");
        }


        [HttpGet("restaurantprofile")]
        public IActionResult RestaurantProfile()
        {
            if (!checkSession())
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Lấy thông tin nhà hàng từ cơ sở dữ liệu
                string restaurantJson = HttpContext.Session.GetString("restaurant");
                var restaurant = JsonConvert.DeserializeObject<Restaurant>(restaurantJson);

                if (restaurant == null)
                {
                    TempData["ErrorMessage"] = "Restaurant not found.";
                    return RedirectToAction("Dashboard");
                }

                // Truyền thông tin nhà hàng vào ViewBag
                ViewBag.Restaurant = restaurant;
                return View(restaurant);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while fetching restaurant profile.";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Dashboard");
            }
        }
        [HttpGet("edit-profile")]
        public IActionResult EditRestaurantProfile()
        {
            if (!checkSession())
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Lấy thông tin nhà hàng từ cơ sở dữ liệu
                string restaurantJson = HttpContext.Session.GetString("restaurant");
                var restaurant = JsonConvert.DeserializeObject<Restaurant>(restaurantJson);

                if (restaurant == null)
                {
                    TempData["ErrorMessage"] = "Restaurant not found.";
                    return RedirectToAction("Dashboard");
                }

                return View(restaurant);  // Pass the restaurant object directly to the view
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while fetching restaurant profile for editing.";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Dashboard");
            }
        }



        [HttpPost("edit-profile")]
        public IActionResult EditRestaurantProfile(Restaurant restaurant)
        {
            if (!checkSession())
            {
                return RedirectToAction("Login");
            }

            try
            {
                var existingRestaurant = _context.Restaurants.FirstOrDefault(r => r.ResId == restaurant.ResId);

                if (existingRestaurant == null)
                {
                    TempData["ErrorMessage"] = "Restaurant not found.";
                    return RedirectToAction("Dashboard");
                }

                // Cập nhật thông tin nhà hàng
                existingRestaurant.NameRes = restaurant.NameRes;
                existingRestaurant.Address = restaurant.Address;
                existingRestaurant.Phone = restaurant.Phone;
                existingRestaurant.Email = restaurant.Email;
                existingRestaurant.Description = restaurant.Description;
                existingRestaurant.Status = restaurant.Status;

                // Cập nhật thông tin vào cơ sở dữ liệu
                _context.Restaurants.Update(existingRestaurant);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Restaurant profile updated successfully!";
                return RedirectToAction("RestaurantProfile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the restaurant profile.";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Dashboard");
            }
        }
        // 1. Hiển thị danh sách món ăn
        [HttpGet("menulist")]
        public IActionResult MenuList()
        {
            if (!checkSession())
                return RedirectToAction("Login");

            var menus = _context.Menus.Include(m => m.CategoryMenu).Include(m => m.Restaurant).ToList();
            return View(menus);
        }

        [HttpGet("menu/create")]
        public IActionResult CreateMenu()
        {
            if (!checkSession())
                return RedirectToAction("Login");

            ViewBag.Categories = _context.CategoryMenus.ToList();  // Lấy danh mục từ CSDL
            ViewBag.Restaurants = _context.Restaurants.ToList();   // Lấy danh sách nhà hàng

            return View();
        }

        [HttpPost("menu/create")]
        public IActionResult CreateMenu(Menu menu)
        {
            if (!checkSession())
                return RedirectToAction("Login");

            try
            {
                if (menu == null || string.IsNullOrEmpty(menu.DishName) || menu.Price <= 0 || menu.Quantity <= 0)
                {
                    TempData["ErrorMessage"] = "Invalid data! Please fill all required fields.";
                    return RedirectToAction("CreateMenu");
                }

                _context.Menus.Add(menu);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Dish added successfully!";
                return RedirectToAction("MenuList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating dish!";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("CreateMenu");
            }
        }

        // Hiển thị form chỉnh sửa món ăn
        [HttpGet("menu/edit/{id}")]
        public IActionResult EditMenu(int id)
        {
            if (!checkSession())
                return RedirectToAction("Login");

            var menu = _context.Menus.FirstOrDefault(m => m.MenuId == id);
            if (menu == null)
            {
                TempData["ErrorMessage"] = "Dish not found!";
                return RedirectToAction("MenuList");
            }

            // Load danh mục và nhà hàng
            ViewBag.Categories = _context.CategoryMenus.ToList();
            ViewBag.Restaurants = _context.Restaurants.ToList();

            return View(menu);
        }

        // Xử lý cập nhật món ăn
        [HttpPost("menu/edit")]
        [ValidateAntiForgeryToken]
        public IActionResult EditMenu(Menu menu)
        {
            if (!checkSession())
                return RedirectToAction("Login");

            try
            {
                var existingMenu = _context.Menus.Find(menu.MenuId);
                if (existingMenu == null)
                {
                    TempData["ErrorMessage"] = "Dish not found!";
                    return RedirectToAction("MenuList");
                }

                // Cập nhật dữ liệu
                existingMenu.DishName = menu.DishName;
                existingMenu.Price = menu.Price;
                existingMenu.Description = menu.Description;
                existingMenu.Status = menu.Status;
                existingMenu.CategoryMenuId = menu.CategoryMenuId;
                existingMenu.RestaurantId = menu.RestaurantId;
                existingMenu.Quantity = menu.Quantity;
                existingMenu.Image = menu.Image;

                _context.Menus.Update(existingMenu);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Dish updated successfully!";
                return RedirectToAction("MenuList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating dish!";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("MenuList");
            }
        }


        [HttpGet("menu/confirm-delete/{id}")]
        public IActionResult ConfirmDeleteMenu(int id)  // Phương thức GET để hiển thị xác nhận xóa
        {
            if (!checkSession())
                return RedirectToAction("Login");

            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                TempData["ErrorMessage"] = "Dish not found!";
                return RedirectToAction("MenuList");
            }

            return View(menu);
        }

        [HttpPost("menu/delete/{id}")]
        public IActionResult DeleteMenuConfirmed(int id)  // Phương thức POST để thực hiện xóa
        {
            if (!checkSession())
                return RedirectToAction("Login");

            try
            {
                var menu = _context.Menus.Find(id);
                if (menu == null)
                {
                    TempData["ErrorMessage"] = "Dish not found!";
                    return RedirectToAction("MenuList");
                }

                _context.Menus.Remove(menu);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Dish deleted successfully!";
                return RedirectToAction("MenuList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting dish!";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("MenuList");
            }
        }

    }
}
