using System.ComponentModel.DataAnnotations;

namespace LaundryManagement.Models.Entities
{
    public class PricelistJasa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JasaId { get; set; }

        [Required]
        public int TipeLayanan { get; set; }

        [Required]
        public decimal Harga { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Jasa? Jasa { get; set; }
    }
}