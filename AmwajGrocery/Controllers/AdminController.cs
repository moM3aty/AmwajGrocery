using AmwajGrocery.Data;
using AmwajGrocery.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Claims;

namespace AmwajGrocery.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- تسجيل الدخول ---
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Dashboard");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "بيانات الدخول غير صحيحة";
            return View();
        }

        [Authorize]
        public IActionResult Dashboard() => View();

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ==========================================
        // === إدارة المنتجات ===
        // ==========================================
        [Authorize]
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> ProductForm(int? id)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            if (id == null) return View(new Product());
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProduct(Product model, IFormFile? ImageFile)
        {
            // إزالة التحقق من ImageUrl لأنه قد يكون فارغاً عند التعديل
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    // --- إضافة منتج جديد ---
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        model.ImageUrl = await SaveImage(ImageFile);
                    }
                    _context.Products.Add(model);
                }
                else
                {
                    // --- تعديل منتج موجود ---
                    var existingProduct = await _context.Products.FindAsync(model.Id);
                    if (existingProduct == null) return NotFound();

                    // تحديث البيانات
                    existingProduct.NameAr = model.NameAr;
                    existingProduct.NameEn = model.NameEn;
                    existingProduct.Description = model.Description;
                    existingProduct.Price = model.Price;
                    existingProduct.OldPrice = model.OldPrice;
                    existingProduct.CategoryId = model.CategoryId;
                    existingProduct.InStock = model.InStock;
                    existingProduct.IsHotDeal = model.IsHotDeal;
                    existingProduct.IsBestSeller = model.IsBestSeller;

                    // تحديث الصورة فقط إذا تم رفع صورة جديدة
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        existingProduct.ImageUrl = await SaveImage(ImageFile);
                    }

                    _context.Products.Update(existingProduct);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Products");
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View("ProductForm", model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null) { _context.Products.Remove(product); await _context.SaveChangesAsync(); }
            return RedirectToAction("Products");
        }

        // ==========================================
        // === إدارة الفئات (تم الإصلاح) ===
        // ==========================================
        [Authorize]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();
            return View(categories);
        }

        [Authorize]
        public async Task<IActionResult> CategoryForm(int? id)
        {
            if (id == null) return View(new Category());
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(Category model, IFormFile? ImageFile)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Products");

            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    // إضافة جديد
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        model.ImageUrl = await SaveImage(ImageFile);
                    }
                    _context.Categories.Add(model);
                }
                else
                {
                    // تعديل موجود (الطريقة الصحيحة)
                    var existingCategory = await _context.Categories.FindAsync(model.Id);
                    if (existingCategory == null) return NotFound();

                    existingCategory.NameAr = model.NameAr;
                    existingCategory.NameEn = model.NameEn; // هذا السطر يضمن حفظ الاسم الإنجليزي

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        existingCategory.ImageUrl = await SaveImage(ImageFile);
                    }

                    _context.Categories.Update(existingCategory);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Categories");
            }
            return View("CategoryForm", model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null) { _context.Categories.Remove(category); await _context.SaveChangesAsync(); }
            return RedirectToAction("Categories");
        }

        // --- دالة مساعدة لحفظ الصور ---
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return "images/" + uniqueFileName;
        }

        // ==========================================
        // === إدارة الطلبات ===
        // ==========================================
        [Authorize]
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders.Include(o => o.OrderItems).OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null) { order.Status = status; await _context.SaveChangesAsync(); }
            return RedirectToAction("Orders");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null) { _context.Orders.Remove(order); await _context.SaveChangesAsync(); }
            return RedirectToAction("Orders");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
        {
            if (orderDto == null || orderDto.Items == null || !orderDto.Items.Any()) return BadRequest("Invalid data");
            var newOrder = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = orderDto.TotalAmount,
                Status = OrderStatus.NotResponded,
                OrderItems = new List<OrderItem>()
            };
            foreach (var item in orderDto.Items)
            {
                newOrder.OrderItems.Add(new OrderItem { ProductName = item.Name, Quantity = item.Qty, Price = item.Price });
            }
            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();
            return Ok(new { orderId = newOrder.Id });
        }

        // ==========================================
        // === استيراد الإكسل ===
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("يرجى رفع ملف.");
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var categoryName = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(categoryName)) continue;

                        var category = await _context.Categories.FirstOrDefaultAsync(c => c.NameAr == categoryName);
                        if (category == null)
                        {
                            category = new Category { NameAr = categoryName, NameEn = categoryName, ImageUrl = "images/default-cat.webp" };
                            _context.Categories.Add(category);
                            await _context.SaveChangesAsync();
                        }

                        var product = new Product
                        {
                            NameAr = worksheet.Cells[row, 2].Value?.ToString()?.Trim(),
                            NameEn = worksheet.Cells[row, 3].Value?.ToString()?.Trim(),
                            Description = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                            Price = Convert.ToDecimal(worksheet.Cells[row, 5].Value),
                            CategoryId = category.Id,
                            ImageUrl = worksheet.Cells[row, 7].Value?.ToString()?.Trim(),
                            InStock = worksheet.Cells[row, 8].Value?.ToString()?.ToLower()?.Trim() == "true",
                            OldPrice = worksheet.Cells[row, 9].Value != null ? Convert.ToDecimal(worksheet.Cells[row, 9].Value) : null,
                            IsBestSeller = worksheet.Cells[row, 10].Value?.ToString()?.ToLower()?.Trim() == "true",
                            IsHotDeal = worksheet.Cells[row, 11].Value?.ToString()?.ToLower()?.Trim() == "true"
                        };
                        _context.Products.Add(product);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Dashboard");
        }

        // DTOs
        public class OrderDto { public decimal TotalAmount { get; set; } public List<OrderItemDto> Items { get; set; } }
        public class OrderItemDto { public string Name { get; set; } public int Qty { get; set; } public decimal Price { get; set; } }
    }
}