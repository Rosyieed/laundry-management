using System.ComponentModel.DataAnnotations;

namespace LaundryManagement.Models.Entities
{
    public class Jasa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string NamaJasa { get; set; }

        [Required]
        public required string Satuan { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<PricelistJasa>? PricelistJasas { get; set; }
    }
}