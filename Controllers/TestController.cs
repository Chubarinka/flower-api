using Microsoft.AspNetCore.Mvc;

namespace FlowerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
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
    public IActionResult CheckDatabase([FromServices] DatabaseContext dbContext)
    {
        try
        {
            // Проверяем подключение к БД
            var canConnect = dbContext.Database.CanConnect();
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
}