using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Model
{
    [Table("user")]
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public string Status { get; set; } = "A";
    }
}
