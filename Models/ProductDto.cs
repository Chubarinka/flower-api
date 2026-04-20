using System.ComponentModel.DataAnnotations;

namespace FlowerApi.Models;

public class ProductDto
{
    [Required(ErrorMessage = "Название товара обязательно")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно содержать от 3 до 100 символов")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Состав букета обязателен")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Состав должен содержать от 10 до 500 символов")]
    public string Composition { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Цена обязательна")]
    [Range(1000, 50000, ErrorMessage = "Цена должна быть от 1000 до 50000 рублей")]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "Артикул обязателен")]
    [RegularExpression(@"^[A-Z]{2,3}-\d{3}$", ErrorMessage = "Формат артикула: FLW-001")]
    public string SKU { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Выберите хотя бы один тип цветка")]
    public List<string> Flowers { get; set; } = new();
    
    public string? CompositionType { get; set; }
    
    public List<string> Occasion { get; set; } = new();
    
    // Изображение в формате base64
    public string? ImageBase64 { get; set; }
}