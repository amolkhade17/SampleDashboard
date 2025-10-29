using AdminDashboard.Application.DTOs;
using AdminDashboard.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminDashboard.Web.Controllers;

[Authorize]
public class ProductController : BaseController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    // GET: Product/Index
    public async Task<IActionResult> Index(string? searchTerm, string? category)
    {
        IEnumerable<ProductDto> products;

        if (!string.IsNullOrWhiteSpace(searchTerm) || !string.IsNullOrWhiteSpace(category))
        {
            products = await _productService.SearchProductsAsync(searchTerm, category);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Category = category;
        }
        else
        {
            products = await _productService.GetAllProductsAsync();
        }

        // Get distinct categories for filter dropdown
        var allProducts = await _productService.GetAllProductsAsync();
        ViewBag.Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

        return View(products);
    }

    // GET: Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            TempData["ErrorMessage"] = "Product not found";
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    // GET: Product/Create
    public async Task<IActionResult> Create()
    {
        // Get categories for dropdown
        var allProducts = await _productService.GetAllProductsAsync();
        ViewBag.Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
        
        return View();
    }

    // POST: Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto model)
    {
        if (!ModelState.IsValid)
        {
            var allProducts = await _productService.GetAllProductsAsync();
            ViewBag.Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            return View(model);
        }

        try
        {
            await _productService.CreateProductAsync(model, GetCurrentUserName());
            TempData["SuccessMessage"] = "Product created successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error creating product: {ex.Message}");
            var allProducts = await _productService.GetAllProductsAsync();
            ViewBag.Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            return View(model);
        }
    }

    // GET: Product/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            TempData["ErrorMessage"] = "Product not found";
            return RedirectToAction(nameof(Index));
        }

        var model = new UpdateProductDto
        {
            ProductId = product.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive
        };

        // Get categories for dropdown
        var allProducts = await _productService.GetAllProductsAsync();
        ViewBag.Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

        return View(model);
    }

    // POST: Product/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductDto model)
    {
        if (id != model.ProductId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var allProducts = await _productService.GetAllProductsAsync();
            ViewBag.Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            return View(model);
        }

        try
        {
            await _productService.UpdateProductAsync(model, GetCurrentUserName());
            TempData["SuccessMessage"] = "Product updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error updating product: {ex.Message}");
            var allProducts = await _productService.GetAllProductsAsync();
            ViewBag.Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            return View(model);
        }
    }

    // POST: Product/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            TempData["SuccessMessage"] = "Product deleted successfully";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting product: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
