using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ReservationsManagement.DTO;
using ReservationsManagement.Models;

namespace ReservationsManagement.Controllers.customer
{
    [Route("customer")]
    public class CustomerController : Controller
    {
        private readonly ReservationsManagementContext _context;
        private readonly IWebHostEnvironment _environment;
        public CustomerController(ReservationsManagementContext context, IWebHostEnvironment environment)
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
                string isCustomerAuthenticated = httpContext.Session.GetString("customer");
                if (string.IsNullOrEmpty(isCustomerAuthenticated))
                {
                    checkS = false;
                }
            }

            return checkS;
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            ViewBag.IsLogin = checkSession();
            var session = HttpContext.Session;

            if (session.Keys.Contains("customer"))
            {
                session.Remove("customer");
            }
            return RedirectToAction("Login");
        }
        [HttpGet("home")]
        public IActionResult Home()
        {
            ViewBag.IsLogin = checkSession();

            // ✅ Lấy thông tin Customer từ Session nếu đã đăng nhập
            string customerJson = HttpContext.Session.GetString("customer");
            if (!string.IsNullOrEmpty(customerJson))
            {
                ViewBag.Customer = JsonConvert.DeserializeObject<Customer>(customerJson);
            }
            else
            {
                ViewBag.Customer = null;
            }

            // ✅ Lấy danh sách menu từ database
            var menuList = _context.Menus.Include(m => m.CategoryMenu)
                                         .Include(m => m.Restaurant)
                                         .ToList();
            ViewBag.MenuList = menuList;

            // ✅ Lấy danh sách danh mục món ăn
            var categoryMenuList = _context.CategoryMenus.ToList();
            ViewBag.CategoryMenuList = categoryMenuList;

            return View();
        }
        [HttpGet("menu")]
        public IActionResult Menu(string search = "", int categoryId = 0, string sortOrder = "default")
        {
            ViewBag.IsLogin = checkSession();

            // ✅ Lấy danh sách danh mục món ăn
            ViewBag.CategoryMenuList = new SelectList(_context.CategoryMenus, "CategoryMenuId", "CategoryMenuName", ViewBag.CurrentCategory);


            // ✅ Truy vấn danh sách món ăn
            var menuList = _context.Menus
                                   .Include(m => m.CategoryMenu)
                                   .Include(m => m.Restaurant)
                                   .AsQueryable();

            // 🔍 Tìm kiếm theo tên món ăn
            if (!string.IsNullOrEmpty(search))
            {
                menuList = menuList.Where(m => m.DishName.Contains(search));
            }

            // 🔍 Lọc theo danh mục món ăn
            if (categoryId > 0)
            {
                menuList = menuList.Where(m => m.CategoryMenuId == categoryId);
            }

            ViewBag.SortOrderList = new List<SelectListItem>
{
    new SelectListItem { Value = "default", Text = "Sắp xếp" },
    new SelectListItem { Value = "price_asc", Text = "Giá thấp đến cao" },
    new SelectListItem { Value = "price_desc", Text = "Giá cao đến thấp" }
};


            ViewBag.MenuList = menuList.ToList(); // Truyền danh sách vào View
            ViewBag.CurrentSearch = search; // Giữ lại giá trị tìm kiếm
            ViewBag.CurrentCategory = categoryId; // Giữ lại giá trị danh mục đã chọn
            ViewBag.CurrentSortOrder = sortOrder; // Giữ lại giá trị sắp xếp đã chọn

            return View();
        }



        [HttpGet("menu/details")]
        public IActionResult MenuDetails(int id) // Nhận tham số từ QueryString
        {
            ViewBag.IsLogin = checkSession();

            var menuItem = _context.Menus
                .Include(m => m.CategoryMenu)
                .FirstOrDefault(m => m.MenuId == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            // ✅ Lấy danh sách món ăn cùng loại (liên quan)
            ViewBag.RelatedMenus = _context.Menus
                .Where(m => m.CategoryMenuId == menuItem.CategoryMenuId && m.MenuId != id)
                .Take(4)
                .ToList();

            return View("Menu_details", menuItem);
        }




        [HttpGet("authenticate/login")]
        public IActionResult Login()
        {
            ViewBag.IsLogin = checkSession();
            return View();
        }


        [HttpPost("authenticate/login")]
        public IActionResult Login(string username, string password)
        {
            ViewBag.IsLogin = checkSession();
            if (_context != null)
            {
                if (username == string.Empty || password == string.Empty)
                {
                    ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không được bỏ trống!";
                    return View();
                }
                else
                {
                    Customer customer = _context.Customers.FirstOrDefault(x => x.Username.Equals(username) && x.Password.Equals(password));

                    if (customer != null)
                    {
                        if (customer.Status.Equals("Deactive"))
                        {
                            ViewBag.Message = "Tài khoản đang bị vô hiệu hóa.";
                            return View();
                        }
                        string customerSession = JsonConvert.SerializeObject(customer);
                        HttpContext.Session.SetString("customer", customerSession);
                        return RedirectToAction("Home");
                    }
                    else
                    {
                        ViewBag.Message = "Tên đăng nhập hoặc mật khẩu sai! vui lòng thử lại";
                        return View();
                    }
                }
            }
            return View();
        }

        [HttpGet("authenticate/register")]
        public IActionResult Register()
        {
            ViewBag.IsLogin = checkSession();
            return View();
        }


        [HttpPost("authenticate/register")]
        public IActionResult Register(string name, string phone, string email, DateTime dob, string username, string password, string repassword, string gender)
        {
            ViewBag.IsLogin = checkSession();
            bool checkInput = true;

            if (!Utils.Validation.IsUsernameUnique(username, _context))
            {
                ViewBag.UsernameError = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
                checkInput = false;
            }

            if (!Utils.Validation.IsEmailValid(email))
            {
                ViewBag.EmailError = "Định dạng email không hợp lệ.";
                checkInput = false;
            }

            if (!Utils.Validation.IsPasswordValid(password))
            {
                ViewBag.PasswordError = "Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ thường, chữ hoa và một số.";
                checkInput = false;
            }

            if (password != repassword)
            {
                ViewBag.RepasswordError = "Mật khẩu và nhập lại mật khẩu không khớp.";
                checkInput = false;
            }
            if (checkInput == true)
            {

                Customer customer = new Customer()
                {
                    Name = name,
                    Phone = phone,
                    Email = email,
                    Gender = gender,
                    Dob = dob,
                    Username = username,
                    Password = password,
                    Status = "Active"
                };

                _context.Customers.Add(customer);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View();
        }

        // PROFILE VIEW
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            ViewBag.IsLogin = checkSession();
            if (checkSession() == false)
            {
                return RedirectToAction("Login");
            }
            int id = 0;
            string customerJson = HttpContext.Session.GetString("customer");
            if (!string.IsNullOrEmpty(customerJson))
            {
                Customer customerSession = JsonConvert.DeserializeObject<Customer>(customerJson);
                id = customerSession.CustomerId;
            }
            Customer customer = _context.Customers.FirstOrDefault(x => x.CustomerId == id);
            return View(customer);
        }
        [HttpGet("profile/edit")]
        public IActionResult EditProfile()
        {
            ViewBag.IsLogin = checkSession();

            if (!ViewBag.IsLogin)
            {
                return RedirectToAction("Login");
            }

            var customerJson = HttpContext.Session.GetString("customer");
            if (string.IsNullOrEmpty(customerJson))
            {
                return RedirectToAction("Login");
            }

            var customerSession = JsonConvert.DeserializeObject<Customer>(customerJson);
            var customer = _context.Customers.FirstOrDefault(x => x.CustomerId == customerSession.CustomerId);

            if (customer == null)
            {
                return NotFound();
            }

            // ✅ Truyền danh sách vào ViewModel
            var viewModel = new CustomerDTO
            {
                Customer = customer,
                GenderList = new List<SelectListItem>
        {
            new SelectListItem { Value = "Male", Text = "Male" },
            new SelectListItem { Value = "Female", Text = "Female" }
        }
            };

            return View(viewModel);
        }



        [HttpPost("profile/edit")]
        public async Task<IActionResult> EditProfile(int id, string name, DateTime dob, string gender, string phone, string email, IFormFile fileUpload)
        {
            ViewBag.IsLogin = checkSession();
            if (checkSession() == false)
            {
                return RedirectToAction("Login");
            }
            Customer customer = _context.Customers.FirstOrDefault(x => x.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            customer.Name = name;
            customer.Gender = gender;
            customer.Phone = phone;
            customer.Email = email;
            customer.Dob = dob;

            //// Upload file 
            string fileURL = string.Empty;
            if (fileUpload != null && fileUpload.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images/avatar");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileUpload.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(fileStream);
                    fileURL = "/Images/avatar/" + uniqueFileName;
                }
                customer.Image = fileURL;
            }

            _context.Customers.Update(customer);
            _context.SaveChanges();
            return RedirectToAction("Profile");
        }
        [HttpGet("create-order")]
        public IActionResult CreateOrder()
        {
            ViewBag.IsLogin = checkSession();
            // Lấy danh sách món ăn từ cơ sở dữ liệu để hiển thị trong popup chọn món
            ViewBag.MenuList = _context.Menus.ToList();
            return View();
        }


        [HttpPost("create-order")]
        public IActionResult CreateOrder(Order order, string orderDetailsJson)
        {
            // Kiểm tra session
            string customerJson = HttpContext.Session.GetString("customer");
            if (string.IsNullOrEmpty(customerJson))
            {
                TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction("Login");
            }

            var customer = JsonConvert.DeserializeObject<Customer>(customerJson);
            order.CustomerId = customer.CustomerId;

            // Giải mã orderDetails từ JSON
            List<OrderDetail> orderDetails = JsonConvert.DeserializeObject<List<OrderDetail>>(orderDetailsJson);

            if (orderDetails == null || orderDetails.Count == 0)
            {
                ViewBag.ErrorMess = "Please select at least one dish.";
                ViewBag.MenuList = _context.Menus.ToList();
                return View(order);
            }


            try
            {
                // Lưu đơn hàng
                order.CreateDate = DateTime.Now;
                order.StatusOrder = "Pending";
                _context.Orders.Add(order);
                _context.SaveChanges(); // Lưu đơn hàng để lấy OrderId

                foreach (var detail in orderDetails)
                {
                    detail.OrderId = order.OrderId;  // Liên kết chi tiết với đơn hàng
                    if (detail.Price == null || detail.Quantity <= 0)  // Kiểm tra Price và Quantity hợp lệ
                    {
                        detail.TotalPrice = 0; // Nếu không hợp lệ thì đặt giá trị TotalPrice = 0
                    }
                    else
                    {
                        detail.TotalPrice = detail.Quantity * detail.Price;  // Tính tổng giá mỗi món
                    }

                    _context.OrderDetails.Add(detail);  // Thêm chi tiết vào bảng OrderDetails
                    _context.SaveChanges();
                }


             

                TempData["SuccessMessage"] = "Create order successfully!";
                return RedirectToAction("Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while creating the order.";
                return RedirectToAction("CreateOrder");
            }
        }



        [HttpGet("orderlist")]
        public IActionResult OrderList()
        {
            ViewBag.IsLogin = checkSession();

            if (!ViewBag.IsLogin)
            {
                return RedirectToAction("Login");
            }

            try
            {
                // ✅ Lấy thông tin khách hàng từ session
                string customerJson = HttpContext.Session.GetString("customer");
                if (!string.IsNullOrEmpty(customerJson))
                {
                    var customer = JsonConvert.DeserializeObject<Customer>(customerJson);

                    if (customer != null && customer.CustomerId > 0)
                    {
                        var orders = _context.Orders
                                             .Where(o => o.CustomerId == customer.CustomerId)
                                             .Include(o => o.OrderDetails)
                                             .ToList();

                        ViewBag.OrderList = orders ?? new List<Order>();
                        ViewBag.Customer = customer;  // ✅ Truyền thông tin khách hàng vào ViewBag

                        if (!orders.Any())
                        {
                            ViewBag.ErrorMessage = "You have no orders.";
                        }
                    }
                    else
                    {
                        ViewBag.OrderList = new List<Order>();
                        ViewBag.ErrorMessage = "Error retrieving customer information.";
                    }
                }
                else
                {
                    ViewBag.OrderList = new List<Order>();
                    ViewBag.ErrorMessage = "You need to log in to view your orders.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.OrderList = new List<Order>();
                ViewBag.ErrorMessage = "An error occurred while fetching your orders. Please try again later.";
                Console.WriteLine($"Error: {ex.Message}");
            }

            return View();
        }


        [HttpGet("orderdetails/{orderId}")]
        public IActionResult OrderDetails(int orderId)
        {
            ViewBag.IsLogin = checkSession();

            if (checkSession() == false)
            {
                return RedirectToAction("Login");
            }

            // Tìm đơn hàng theo OrderId
            var order = _context.Orders
                                .Where(o => o.OrderId == orderId)
                                .Include(o => o.OrderDetails)
                                .ThenInclude(od => od.Menu) 
                                .FirstOrDefault();

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.OrderDetails = order.OrderDetails; 
            return View(order);
        }

    }
}
