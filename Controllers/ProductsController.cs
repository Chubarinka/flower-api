using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowerApi.Models;

namespace FlowerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProductsController(DatabaseContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: api/products/search?search=роза
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<object>>> SearchProducts([FromQuery] string search)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(search) || search.Length < 2)
                return Ok(new List<object>());

            var allProducts = await _context.Products.ToListAsync();

            var results = allProducts
                .Where(p =>
                    (p.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Composition?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Flowers != null && p.Flowers.Any(f => f.Contains(search, StringComparison.OrdinalIgnoreCase))) ||
                    (p.Occasion != null && p.Occasion.Any(o => o.Contains(search, StringComparison.OrdinalIgnoreCase))))
                .Take(5)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.ImageUrl
                })
                .ToList();

            return Ok(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в SearchProducts: {ex.Message}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<object>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? sort = null,
        [FromQuery] string? flowers = null,
        [FromQuery] string? composition = null,
        [FromQuery] string? occasion = null)
    {
        try
        {
            var allProducts = await _context.Products.ToListAsync();

            var filteredProducts = allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredProducts = filteredProducts.Where(p =>
                    (p.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Composition?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (minPrice.HasValue)
                filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(flowers))
            {
                var flowerList = flowers.Split(',', StringSplitOptions.RemoveEmptyEntries);
                filteredProducts = filteredProducts.Where(p =>
                    p.Flowers != null &&
                    p.Flowers.Any(f => flowerList.Contains(f, StringComparer.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(composition))
            {
                var compositionList = composition.Split(',', StringSplitOptions.RemoveEmptyEntries);
                filteredProducts = filteredProducts.Where(p =>
                    p.CompositionType != null &&
                    compositionList.Contains(p.CompositionType, StringComparer.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(occasion))
            {
                var occasionList = occasion.Split(',', StringSplitOptions.RemoveEmptyEntries);
                filteredProducts = filteredProducts.Where(p =>
                    p.Occasion != null &&
                    p.Occasion.Any(o => occasionList.Contains(o, StringComparer.OrdinalIgnoreCase)));
            }

            filteredProducts = sort switch
            {
                "price-asc" => filteredProducts.OrderBy(p => p.Price),
                "price-desc" => filteredProducts.OrderByDescending(p => p.Price),
                _ => filteredProducts.OrderByDescending(p => p.Id)
            };

            var totalCount = filteredProducts.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var products = filteredProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Composition,
                    p.Price,
                    p.SKU,
                    Flowers = p.Flowers ?? new List<string>(),
                    p.CompositionType,
                    Occasion = p.Occasion ?? new List<string>(),
                    p.ImageUrl
                })
                .ToList();

            return Ok(new
            {
                products,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages,
                    hasNext = page < totalPages
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в GetProducts: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");

            return StatusCode(500, new
            {
                error = "Внутренняя ошибка сервера",
                details = ex.Message
            });
        }
    }

    // GET: api/products/sku/{sku}
    [HttpGet("sku/{sku}")]
    public async Task<ActionResult<bool>> CheckSku(string sku)
    {
        var exists = await _context.Products.AnyAsync(p => p.SKU == sku);
        return Ok(!exists);
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] ProductDto productDto)
    {
        try
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == productDto.SKU);
            if (existingProduct != null)
            {
                return BadRequest(new { error = "Товар с таким артикулом уже существует" });
            }

            string? imageUrl = null;
            if (!string.IsNullOrEmpty(productDto.ImageBase64))
            {
                imageUrl = await SaveImage(productDto.ImageBase64);
            }

            var product = new Product
            {
                Name = productDto.Name,
                Composition = productDto.Composition,
                Price = productDto.Price,
                SKU = productDto.SKU,
                Flowers = productDto.Flowers,
                CompositionType = productDto.CompositionType,
                Occasion = productDto.Occasion,
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Товар успешно добавлен",
                product = new
                {
                    product.Id,
                    product.Name,
                    product.Price,
                    product.SKU,
                    product.ImageUrl
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
        }
    }

    private async Task<string> SaveImage(string base64Image)
    {
        try
        {
            var base64Data = base64Image.Contains(",")
                ? base64Image.Substring(base64Image.IndexOf(",") + 1)
                : base64Image;

            var imageBytes = Convert.FromBase64String(base64Data);

            var fileName = $"product_{Guid.NewGuid()}.jpg";
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            return $"/uploads/{fileName}";
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при сохранении изображения", ex);
        }
    }

    private void DeleteImage(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении изображения: {ex.Message}");
        }
    }
}