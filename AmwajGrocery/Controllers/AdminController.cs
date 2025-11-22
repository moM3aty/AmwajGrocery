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

        // ... (كل الدوال السابقة كما هي: Login, Dashboard, Products, SaveProduct...) ...
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
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, "Admin") };
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

        // المنتجات
        [Authorize]
        public async Task<IActionResult> Products(string search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Products.Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.NameAr.Contains(search) || p.NameEn.Contains(search));
            int totalItems = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchTerm = search;
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
            ModelState.Remove("ImageUrl"); ModelState.Remove("Category");
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    if (ImageFile != null && ImageFile.Length > 0) model.ImageUrl = await SaveImage(ImageFile);
                    _context.Products.Add(model);
                }
                else
                {
                    var existingProduct = await _context.Products.FindAsync(model.Id);
                    if (existingProduct == null) return NotFound();
                    existingProduct.NameAr = model.NameAr;
                    existingProduct.NameEn = model.NameEn;
                    existingProduct.Description = model.Description;
                    existingProduct.DescriptionEn = model.DescriptionEn;
                    existingProduct.Price = model.Price;
                    existingProduct.OldPrice = model.OldPrice;
                    existingProduct.CategoryId = model.CategoryId;
                    existingProduct.InStock = model.InStock;
                    existingProduct.IsHotDeal = model.IsHotDeal;
                    existingProduct.IsBestSeller = model.IsBestSeller;
                    if (ImageFile != null && ImageFile.Length > 0) existingProduct.ImageUrl = await SaveImage(ImageFile);
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

        // الفئات
        [Authorize]
        public async Task<IActionResult> Categories(string search, int page = 1)
        {
            int pageSize = 6;
            var query = _context.Categories.Include(c => c.Products).AsQueryable();
            if (!string.IsNullOrEmpty(search)) query = query.Where(c => c.NameAr.Contains(search) || c.NameEn.Contains(search));
            int totalItems = await query.CountAsync();
            var categories = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchTerm = search;
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
            ModelState.Remove("ImageUrl"); ModelState.Remove("Products");
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    if (ImageFile != null && ImageFile.Length > 0) model.ImageUrl = await SaveImage(ImageFile);
                    _context.Categories.Add(model);
                }
                else
                {
                    var existingCategory = await _context.Categories.FindAsync(model.Id);
                    if (existingCategory == null) return NotFound();
                    existingCategory.NameAr = model.NameAr;
                    existingCategory.NameEn = model.NameEn;
                    if (ImageFile != null && ImageFile.Length > 0) existingCategory.ImageUrl = await SaveImage(ImageFile);
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

        // الطلبات
        [Authorize]
        public async Task<IActionResult> Orders(string search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Orders.Include(o => o.OrderItems).OrderByDescending(o => o.OrderDate).AsQueryable();
            if (!string.IsNullOrEmpty(search)) query = query.Where(o => o.Id.ToString().Contains(search));
            int totalItems = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchTerm = search;
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "يرجى اختيار ملف لرفعه.";
                return RedirectToAction("Dashboard");
            }

            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;
                        int addedCount = 0;

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
                                IsHotDeal = worksheet.Cells[row, 11].Value?.ToString()?.ToLower()?.Trim() == "true",
                                DescriptionEn = worksheet.Cells[row, 12].Value?.ToString()?.Trim()
                            };
                            _context.Products.Add(product);
                            addedCount++;
                        }
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = $"تم استيراد {addedCount} منتج بنجاح!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء الاستيراد: " + ex.Message;
            }

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportProducts()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var products = await _context.Products.Include(p => p.Category).ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");

                // العناوين
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "NameAr";
                worksheet.Cells[1, 3].Value = "NameEn";
                worksheet.Cells[1, 4].Value = "Description";
                worksheet.Cells[1, 5].Value = "Price";
                worksheet.Cells[1, 6].Value = "Category";
                worksheet.Cells[1, 7].Value = "ImageUrl";
                worksheet.Cells[1, 8].Value = "Stock";
                worksheet.Cells[1, 9].Value = "OldPrice";
                worksheet.Cells[1, 10].Value = "IsBestSeller";
                worksheet.Cells[1, 11].Value = "IsHotDeal";
                worksheet.Cells[1, 12].Value = "DescriptionEn";

                // البيانات
                for (int i = 0; i < products.Count; i++)
                {
                    var p = products[i];
                    worksheet.Cells[i + 2, 1].Value = p.Id;
                    worksheet.Cells[i + 2, 2].Value = p.NameAr;
                    worksheet.Cells[i + 2, 3].Value = p.NameEn;
                    worksheet.Cells[i + 2, 4].Value = p.Description;
                    worksheet.Cells[i + 2, 5].Value = p.Price;
                    worksheet.Cells[i + 2, 6].Value = p.Category?.NameAr;
                    worksheet.Cells[i + 2, 7].Value = p.ImageUrl;
                    worksheet.Cells[i + 2, 8].Value = p.InStock;
                    worksheet.Cells[i + 2, 9].Value = p.OldPrice;
                    worksheet.Cells[i + 2, 10].Value = p.IsBestSeller;
                    worksheet.Cells[i + 2, 11].Value = p.IsHotDeal;
                    worksheet.Cells[i + 2, 12].Value = p.DescriptionEn;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"Products-{DateTime.Now:yyyyMMdd}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

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

        public class OrderDto { public decimal TotalAmount { get; set; } public List<OrderItemDto> Items { get; set; } }
        public class OrderItemDto { public string Name { get; set; } public int Qty { get; set; } public decimal Price { get; set; } }
    }
}