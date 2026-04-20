using System.ComponentModel.DataAnnotations;

namespace FlowerApi.Models;

public class Favorite
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    public Product? Product { get; set; }
}