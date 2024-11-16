using Microsoft.AspNetCore.Mvc;
using TechNest.Services;

namespace TechNest.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 8;

        public StoreController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int pageIndex)
        {
            var products = context.Products.OrderByDescending(p => p.Id).ToList();

            ViewBag.Products = products;

            return View();
        }
    }
}
