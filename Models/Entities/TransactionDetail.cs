using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryManagement.Models.Entities
{
    [Table("TransactionDetails")]
    public class TransactionDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TransactionId { get; set; }

        [Required]
        public int PricelistJasaId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; } // Contoh: 2.5 (kg) atau 1 (pcs)

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAtTime { get; set; } // Harga saat transaksi dilakukan

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0; // Nominal diskon untuk item ini saja

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } // Rumus: (Qty * PriceAtTime) - Discount

        // Navigation Properties
        public Transaction? Transaction { get; set; }
        public PricelistJasa? PricelistJasa { get; set; }
    }
}