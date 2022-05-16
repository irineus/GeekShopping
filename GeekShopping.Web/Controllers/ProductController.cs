using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }
        
        public async Task<IActionResult> ProductIndex()
        {
            var products = await _productService.FindAllProducts();
            return View(products);
        }

        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.CreateProduct(model);
                if (response is not null) return RedirectToAction(nameof(ProductIndex));
            }
            return View(model);
        }

        public async Task<IActionResult> ProductUpdate(long id)
        {
            var model = await _productService.FindProductById(id);
            if (model is null) return NotFound();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProductUpdate(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.UpdateProduct(model);
                if (response is not null) return RedirectToAction(nameof(ProductIndex));
            }
            return View(model);
        }

        public async Task<IActionResult> ProductDelete(long id)
        {
            var model = await _productService.FindProductById(id);
            if (model is null) return NotFound();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductModel model)
        {
            try
            {
                await _productService.DeleteProduct(model.Id);
            }
            catch (HttpRequestException)
            {

                return View(model);
            }
            
            return RedirectToAction(nameof(ProductIndex));
        }
    }
}
