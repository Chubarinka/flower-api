using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowerApi.Models; 
namespace FlowerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly DatabaseContext _context;

    public TestController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            message = "API работает!", 
            timestamp = DateTime.Now,
            status = "Healthy"
        });
    }
    
    [HttpGet("db-check")]
    public IActionResult CheckDatabase()
    {
        try
        {
            var canConnect = _context.Database.CanConnect();
            return Ok(new { 
                canConnect, 
                message = canConnect ? "Database connected" : "Database connection failed" 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("create-database")]
    public async Task<IActionResult> CreateDatabase()
    {
        try
        {
            // Принудительно создаем базу данных и таблицы
            await _context.Database.EnsureCreatedAsync();
            
            // Проверяем, создались ли таблицы
            var productsExist = await _context.Products.AnyAsync();
            
            return Ok(new { 
                success = true, 
                message = "Database and tables created successfully!",
                tablesCreated = true,
                productCount = await _context.Products.CountAsync()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                success = false, 
                error = ex.Message,
                innerError = ex.InnerException?.Message 
            });
        }
    }

    [HttpPost("seed-products")]
    public async Task<IActionResult> SeedProducts()
    {
        try
        {
            // Проверяем, есть ли уже товары
            var existingCount = await _context.Products.CountAsync();
            if (existingCount > 0)
            {
                return Ok(new { 
                    success = false, 
                    message = $"Products already exist ({existingCount} items)", 
                    productCount = existingCount 
                });
            }

            // Создаем список товаров из ваших данных
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Нежность",
                    Composition = "51 роза нежно-розового оттенка, свежий эвкалипт, стильная крафтовая упаковка. Розы из лучших эквадорских садов, срезанные за 24 часа до отправки. Каждый бутон раскрывается медленно, даря радость на протяжении 10-14 дней. В комплекте: именная открытка с вашими словами, инструкция по уходу.",
                    Price = 5500,
                    SKU = "FLW-001",
                    Flowers = new List<string> { "roses" },
                    Occasion = new List<string> { "love", "march8" },
                    CompositionType = "box",
                    ImageUrl = "/uploads/product_16f84671-0062-4238-8506-348a9634ad7e.jpg"
                },
                new Product
                {
                    Name = "Страстный поцелуй",
                    Composition = "61 роза насыщенного красного цвета, элегантная шляпная коробка. Роскошный букет в коробке — цветы, которые не требуют вазы. Розы выращены на плантациях Эквадора, отличаются крупными бутонами и стойкостью до 14 дней. В комплекте: открытка с вашими пожеланиями, подробная инструкция по уходу.",
                    Price = 7600,
                    SKU = "FLW-002",
                    Flowers = new List<string> { "roses" },
                    Occasion = new List<string> { "love", "wow", "march8" },
                    CompositionType = "box",
                    ImageUrl = "/uploads/product_133a84bc-9968-42a0-a97e-7279d7375a26.jpg"
                },
                new Product
                {
                    Name = "Солнечное настроение",
                    Composition = "45 хризантем ярко-желтого цвета, воздушная гипсофила, атласная лента, белая коробка. Яркие хризантемы из Голландии — символ солнца и радости. Стойкие цветы, которые будут радовать до 3 недель. В комплекте: открытка, памятка по уходу за срезкой.",
                    Price = 4500,
                    SKU = "FLW-003",
                    Flowers = new List<string> { "chrysanthemums" },
                    Occasion = new List<string> { "birthday", "home", "march8" },
                    CompositionType = "basket",
                    ImageUrl = "/uploads/product_1a920a32-28e6-4425-9d0b-ff9e56ebebb7.jpg"
                },
                new Product
                {
                    Name = "Экспрессия",
                    Composition = "15 хризантем бордового и оранжевого оттенков, веточки рябины, белая коробка. Уютная композиция в коробке, напоминающая о жаркой осени. Хризантемы из Голландии — королевы осенних букетов. В комплекте: открытка с теплыми словами, инструкция по уходу.",
                    Price = 3200,
                    SKU = "FLW-004",
                    Flowers = new List<string> { "chrysanthemums" },
                    Occasion = new List<string> { "wow", "home" },
                    CompositionType = "bouquet",
                    ImageUrl = "/uploads/product_5b22da55-854e-4352-a8ba-dd20b2497e0e.jpg"
                },
                new Product
                {
                    Name = "Пионовое облако",
                    Composition = "15 махровых пионов нежно-розового цвета, свежая зелень, стильная упаковка. Махровые пионы из голландских теплиц — настоящий символ нежности и изящества. Крупные шапки цветов создают эффект воздушного облака. В комплекте: открытка, подробный гид по уходу.",
                    Price = 4300,
                    SKU = "FLW-005",
                    Flowers = new List<string> { "peonies" },
                    Occasion = new List<string> { "love", "birthday" },
                    CompositionType = "box",
                    ImageUrl = "/uploads/product_239f576c-eb3e-4cfb-96e8-30d44bf0ddc4.jpg"
                },
                new Product
                {
                    Name = "Love Story",
                    Composition = "30 нежно-розовых роз, 7 махровых пиона, веточки эвкалипта. Роскошное сочетание двух главных цветов для признания в любви. Розы Эквадора и пионы Голландии создают идеальный дуэт. В комплекте: именная открытка, инструкция по уходу.",
                    Price = 5000,
                    SKU = "FLW-006",
                    Flowers = new List<string> { "roses", "peonies" },
                    Occasion = new List<string> { "love", "birthday" },
                    CompositionType = "box",
                    ImageUrl = "/uploads/product_bbc466be-ca1e-4358-b069-075ea2135e6a.jpg"
                },
                new Product
                {
                    Name = "Садовый рай",
                    Composition = "5 голубые гортензии, 6 махровых пионов. Цветочный букет, напоминающая летний сад. Гортензии из Голландии и пионы из российских питомников создают атмосферу уюта. В комплекте: открытка, памятка по уходу.",
                    Price = 6200,
                    SKU = "FLW-007",
                    Flowers = new List<string> { "hydrangeas", "peonies" },
                    Occasion = new List<string> { "home", "wow", "birthday" },
                    CompositionType = "box",
                    ImageUrl = "/uploads/product_2358f835-6a69-4bf0-a36e-4c1b2a00d58b.jpg"
                },
                new Product
                {
                    Name = "Моя радость",
                    Composition = "5 роз, 3 веточки ромашки, свежая зелень. Маленький, но очень трогательный букет. Розы Эквадора и ромашки из российских полей — сочетание нежности и искренности. В комплекте: открытка, памятка по уходу.",
                    Price = 1500,
                    SKU = "FLW-008",
                    Flowers = new List<string> { "roses", "daisies" },
                    Occasion = new List<string> { "march8", "home" },
                    CompositionType = "box",
                    ImageUrl = "/uploads/product_f2b44e71-d165-45d7-91fe-6a7c5e48e276.jpg"
                },
                new Product
                {
                    Name = "Рассвет",
                    Composition = "31 роза персикового оттенка, 13 кустовых хризантем, декоративные колосья, крафтовая упаковка. Букет в мягких рассветных тонах для самых тёплых моментов. Розы из эквадорских плантаций отличаются нежным цветом и крупным бутоном.",
                    Price = 8400,
                    SKU = "FLW-010",
                    Flowers = new List<string> { "roses", "chrysanthemums" },
                    Occasion = new List<string> { "love", "march8", "wow" },
                    CompositionType = "box",
                    ImageUrl = "/uploads/product_c348983f-953f-47f4-9e69-190d83c2d6e8.jpg"
                },
                new Product
                {
                    Name = "Снежная сказка",
                    Composition = "7 розовых махровых пионов, 4 голубых пионов сорта «Скай», 8 белых пионов сорта «Дюшес», свежий эвкалипт, декоративная зелень. Истинное произведение флористического искусства — букет из трёх оттенков самых роскошных пионов.",
                    Price = 11000,
                    SKU = "FLW-011",
                    Flowers = new List<string> { "hydrangeas" },
                    Occasion = new List<string> { "wow" },
                    CompositionType = "box",
                    ImageUrl = "/uploads/product_22ae4623-c307-44ac-a0dd-66ed8aa0ccf3.jpg"
                },
                new Product
                {
                    Name = "Белые лилии",
                    Composition = "7 белоснежных лилий, веточки эвкалипта элегантная корзина. Благородные лилии из Голландии — символ чистоты и изысканности. Тонкий аромат наполнит комнату свежестью.",
                    Price = 1900,
                    SKU = "FLW-009",
                    Flowers = new List<string> { "lilies" },
                    Occasion = new List<string> { "love", "home" },
                    CompositionType = "basket",
                    ImageUrl = "/uploads/product_5b290156-aa95-4c17-841f-2f164971e855.jpg"
                },
                new Product
                {
                    Name = "Эквадорский закат",
                    Composition = "35 роз теплого оранжево-желтого градиента, 7 веточек эвкалипта, декоративные колосья, стильная коробка. Букет, вдохновленный закатным солнцем над эквадорскими плантациями.",
                    Price = 7200,
                    SKU = "FLW-015",
                    Flowers = new List<string> { "roses" },
                    Occasion = new List<string> { "march8", "love" },
                    CompositionType = "bouquet",
                    ImageUrl = "/uploads/product_9c92083f-83fd-4278-8a11-fc7613358a84.jpg"
                }
            };

            // Добавляем все товары в базу данных
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                message = $"Successfully added {products.Count} products",
                productCount = products.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                success = false, 
                error = ex.Message,
                innerError = ex.InnerException?.Message 
            });
        }
    }
}