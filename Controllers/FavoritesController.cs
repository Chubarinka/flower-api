using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowerApi.Models;

namespace FlowerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly DatabaseContext _context;

    public FavoritesController(DatabaseContext context)
    {
        _context = context;
    }

    // GET: api/favorites
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetFavorites()
    {
        try
        {
            var favoriteProductIds = await _context.Favorites
                .Select(f => f.ProductId)
                .ToListAsync();
            
            if (!favoriteProductIds.Any())
            {
                return Ok(new List<Product>());
            }
            
            var products = await _context.Products
                .Where(p => favoriteProductIds.Contains(p.Id))
                .ToListAsync();
            
            return Ok(products);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetFavorites: {ex.Message}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    // GET: api/favorites/check/{productId}
    [HttpGet("check/{productId}")]
    public async Task<ActionResult<bool>> CheckFavorite(int productId)
    {
        try
        {
            var exists = await _context.Favorites
                .AnyAsync(f => f.ProductId == productId);
            
            return Ok(exists);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckFavorite: {ex.Message}");
            return StatusCode(500, false);
        }
    }

    // POST: api/favorites/{productId}
    [HttpPost("{productId}")]
    public async Task<ActionResult> AddToFavorite(int productId)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(new { error = "Товар не найден" });
            }

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.ProductId == productId);
            
            if (existing != null)
            {
                return Ok(new { message = "Товар уже в избранном", alreadyExists = true });
            }

            var favorite = new Favorite
            {
                ProductId = productId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар добавлен в избранное", success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddToFavorite: {ex.Message}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
        }
    }

    // DELETE: api/favorites/{productId}
    [HttpDelete("{productId}")]
    public async Task<ActionResult> RemoveFromFavorite(int productId)
    {
        try
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.ProductId == productId);
            
            if (favorite == null)
            {
                return NotFound(new { error = "Товар не найден в избранном" });
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар удален из избранного", success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RemoveFromFavorite: {ex.Message}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
        }
    }
}