using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryManagement.Models.Entities
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string InvoiceNo { get; set; } = string.Empty; // Contoh: INV-260226-00001 Mapping: INV-DDMMYY-XXXXX

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? EstimatedFinishDate { get; set; } // Estimasi jemput
        public DateTime? PickupDate { get; set; } // Tanggal benar-benar diambil

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } // Total kotor sebelum diskon

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDiscount { get; set; } // Akumulasi diskon dari semua detail

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; } // Total bersih (TotalPrice - TotalDiscount)

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Process, Finished, Taken

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid

        [MaxLength(20)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, QRIS

        public string? Notes { get; set; }

        // Audit Columns
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public Customer? Customer { get; set; }
        public ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
    }
}