using Microsoft.AspNetCore.Mvc;
using TechNest.Models;
using TechNest.Services;

namespace TechNest.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;
        private readonly int pageSize = 5;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index(int pageIndex, string? search)
        {
            IQueryable<Product> query = context.Products;

            if(search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            }

            query = query.OrderByDescending(p => p.Id);

            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var products = query.ToList();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;

            ViewData["Search"] = search ?? "";

            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDTO productDTO)
        {
            if (productDTO.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(productDTO);
            }

            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDTO.ImageFile.FileName);

            string imageFullPath = environment.WebRootPath + "/Product-Images/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDTO.ImageFile.CopyTo(stream);
            }

            Product product = new Product()
            {
                Name = productDTO.Name,
                Brand = productDTO.Brand,
                Category = productDTO.Category,
                Price = productDTO.Price,
                Description = productDTO.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now
            };

            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Product");
        }

        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }

            var productDTO = new ProductDTO()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

            return View(productDTO);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDTO productDTO)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

                return View(productDTO);
            }

            string newFileName = product.ImageFileName;
            if(productDTO.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDTO.ImageFile.FileName);

                string imageFullPath = environment.WebRootPath + "/Product-Images/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDTO.ImageFile.CopyTo(stream);
                }

                string oldImageFullPath = environment.WebRootPath + "/Product-Images/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            product.Name = productDTO.Name;
            product.Brand = productDTO.Brand;
            product.Category = productDTO.Category;
            product.Price = productDTO.Price;
            product.Description = productDTO.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();

            return RedirectToAction("Index", "Product");
        }

        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }

            string imageFullPath = environment.WebRootPath + "/Product-Images/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);

            context.Products.Remove(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Product");
        }
    }
}
