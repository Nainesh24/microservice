using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Model;
using ProductService.Model.Request;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ProductController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsAsync()
        {
            try
            {
                var products = _applicationDbContext.Products.ToList();

                var response = new ApiResponse<List<Product>>
                {
                    Status = "OK",
                    Data = products,
                    Message = "Products fetched successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = "ERROR",
                    Data = null,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductAsync(ProductRequest request)
        {
            try
            {
                var existProduct = await _applicationDbContext.Products
                    .FirstOrDefaultAsync(x => x.Name == request.Name);

                if (existProduct != null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Status = "ERROR",
                        Data = null,
                        Message = $"Product already exists with name: {request.Name}"
                    });
                }

                var product = new Product
                {
                    Name = request.Name,
                    Price = request.Price
                };

                await _applicationDbContext.Products.AddAsync(product);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new ApiResponse<Product>
                {
                    Status = "OK",
                    Data = product,
                    Message = "Product created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = "ERROR",
                    Data = null,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductAsync(ProductRequest request, int id)
        {
            try
            {
                var product = await _applicationDbContext.Products
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (product == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Status = "ERROR",
                        Data = null,
                        Message = "Product not found"
                    });
                }

                var existProduct = await _applicationDbContext.Products
                    .AnyAsync(x => x.Name == request.Name && x.Id != id);

                if (existProduct)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Status = "ERROR",
                        Data = null,
                        Message = "Product name already used"
                    });
                }

                product.Name = request.Name;
                product.Price = request.Price;
                product.ModifiedAt = DateTime.UtcNow;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new ApiResponse<Product>
                {
                    Status = "OK",
                    Data = product,
                    Message = "Product updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = "ERROR",
                    Data = null,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _applicationDbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

                if (product == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Status = "ERROR",
                        Data = null,
                        Message = "Product not found"
                    });
                }
                product.ModifiedAt = DateTime.UtcNow;
                product.Status = "D";
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Status = "OK",
                    Data = null,
                    Message = "Product deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = "ERROR",
                    Data = null,
                    Message = ex.Message
                });
            }
        }
    }
}
