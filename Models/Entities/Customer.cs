using System.ComponentModel.DataAnnotations;

namespace LaundryManagement.Models.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(15)]
        public required string PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [MaxLength(100)]
        public required string CreatedBy { get; set; }
        
        public string? UpdatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}