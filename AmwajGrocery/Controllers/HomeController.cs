using AmwajGrocery.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmwajGrocery.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var deals = await _context.Products.Include(p => p.Category).Where(p => p.IsHotDeal && p.InStock).Take(6).ToListAsync();
            if (!deals.Any()) deals = await _context.Products.Include(p => p.Category).Where(p => p.InStock).OrderByDescending(p => p.Id).Take(4).ToListAsync();

            var bestSellers = await _context.Products.Include(p => p.Category).Where(p => p.IsBestSeller && p.InStock).Take(6).ToListAsync();
            if (!bestSellers.Any()) bestSellers = await _context.Products.Include(p => p.Category).Where(p => p.InStock).OrderBy(p => p.Price).Take(4).ToListAsync();

            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();

            ViewBag.Deals = deals;
            ViewBag.BestSellers = bestSellers;
            ViewBag.Categories = categories;

            return View();
        }

        public async Task<IActionResult> Products(string q, int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(p => p.NameAr.Contains(q) || p.NameEn.Contains(q) || p.Category.NameAr.Contains(q));
                ViewBag.SearchQuery = q;
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
                var cat = await _context.Categories.FindAsync(categoryId);
                ViewBag.CategoryNameAr = cat?.NameAr;
                ViewBag.CategoryNameEn = cat?.NameEn;
            }

            var products = await query.ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> LiveSearch(string q)
        {
            if (string.IsNullOrEmpty(q)) return Json(new List<object>());

            var products = await _context.Products
                .Where(p => p.NameAr.Contains(q) || p.NameEn.Contains(q))
                .Select(p => new {
                    id = p.Id,
                    name = p.NameAr,   
                    nameEn = p.NameEn,
                    price = p.Price,
                    image = p.ImageUrl
                })
                .Take(5)
                .ToListAsync();

            return Json(products);
        }

        [HttpPost]
        public IActionResult ChangeLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true }
            );

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}