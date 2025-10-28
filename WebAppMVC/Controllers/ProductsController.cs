using Microsoft.AspNetCore.Mvc;

namespace WebAppMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.OperationTitle = "View Products";
            ViewBag.OperationDescription = "Browse and manage product catalog";
            ViewBag.OperationIcon = "fas fa-boxes";
            ViewBag.BackgroundColor = "#2196f3";

            return View();
        }

        public IActionResult Create()
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.OperationTitle = "Add Product";
            ViewBag.OperationDescription = "Add a new product to inventory";
            ViewBag.OperationIcon = "fas fa-plus-circle";
            ViewBag.BackgroundColor = "#4caf50";

            return View();
        }

        public IActionResult Edit(int id)
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.OperationTitle = "Edit Product";
            ViewBag.OperationDescription = "Modify product information";
            ViewBag.OperationIcon = "fas fa-edit";
            ViewBag.BackgroundColor = "#ff9800";

            return View();
        }

        public IActionResult Delete(int id)
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.OperationTitle = "Delete Product";
            ViewBag.OperationDescription = "Remove product from inventory";
            ViewBag.OperationIcon = "fas fa-trash-alt";
            ViewBag.BackgroundColor = "#f44336";

            return View();
        }

        private bool IsUserAuthenticated()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            return !string.IsNullOrEmpty(token);
        }
    }
}