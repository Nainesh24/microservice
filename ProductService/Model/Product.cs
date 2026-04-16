using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Model
{
    [Table("product")]
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public string Status { get; set; } = "A";
    }
}
