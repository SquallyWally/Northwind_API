using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Northwind.Common.EntityModels.SqlServer;
using Northwind.Mvc.Models;

// Cache thje HTTP Response voor beter schaalbaarheid en response tme
namespace Northwind.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly NorthwindContext _db;

    public HomeController(ILogger<HomeController> logger, NorthwindContext injectionContext)
    {
        _logger = logger;
        _db = injectionContext;
    }

    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Index()
    {
        _logger.LogError("This is a serious error (not really!)");
        _logger.LogWarning("This is your first warning!");

        // Create an View model instance
        HomeIndexViewModel model = new(
            VisitorCount: (new Random()).Next(1, 1001),
            Categories: await _db.Categories.ToListAsync(),
            Products: await _db.Products.ToListAsync()
        );

        return View(model);
    }

    [Route("private")]
    [Authorize(Roles = "Administrators")]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }

    public async Task<IActionResult> ProductDetail(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest("You must pass a product ID in the route, for example, /Home/ProductDetail/21");
        }

        Product? model = await _db.Products.SingleOrDefaultAsync(p => p.ProductId == id);

        if (model == null)
        {
            return NotFound($"ProductId {id} not found.");
        }

        return View(model);
    }

    public IActionResult ProductsThatCostMoreThan(decimal? price)
    {
        if (!price.HasValue)
        {
            return BadRequest(
                "You must pass a product price in the query string, for example, /Home/ProductsThatCostMoreThan?price=50");
        }

        IEnumerable<Product> model = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.UnitPrice > price);

        if (!model.Any())
        {
            return NotFound($"No products cost more than {price:C}.");
        }

        ViewData["MaxPrice"] = price.Value.ToString("C");
        return View(model);
    }
    
    [Route("category")]
    public async Task<IActionResult> Categories()
    {
        IEnumerable<Category> model = await _db.Categories.ToListAsync();
        if (!model.Any())
        {
            return NotFound("There are no Categories");
        }
        
        return View(model);
    }

    [Route("category/{id:int}")]
    public async Task<IActionResult> CategoryDetail(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest("You must pass a category ID in the route, for example, /category/2");
        }

        Category? model = await _db.Categories.SingleOrDefaultAsync(c => c.CategoryId == id);

        if (model == null)
        {
            return NotFound($"CategoryId {id} not found.");
        }

        return View(model);
    }
}