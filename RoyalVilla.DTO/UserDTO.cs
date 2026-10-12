using System.ComponentModel.DataAnnotations;

namespace RoyalVilla.DTO
{
    public class UserDTO
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; } = default!;

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; } = default!;

        [Required]
        [MaxLength(50)]
        public required string Role { get; set; } = "Customer";

    }
}
