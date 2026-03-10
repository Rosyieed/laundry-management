using System.ComponentModel.DataAnnotations;

namespace LaundryManagement.Models.ViewModels
{
    public class TransactionViewModel
    {
        // Pilihan Customer
        public int? CustomerId { get; set; }

        public bool IsNewCustomer { get; set; }

        [MaxLength(100)]
        public string? NewCustomerName { get; set; }

        [MaxLength(15)]
        public string? NewCustomerPhone { get; set; }

        [MaxLength(255)]
        public string? NewCustomerAddress { get; set; }

        // Data Transaksi Utama
        [Required(ErrorMessage = "Tanggal order wajib diisi")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? EstimatedFinishDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid

        [Required]
        [MaxLength(20)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, QRIS, Transfer

        public string? Notes { get; set; }

        // List Detail Jasa yang dibeli
        public List<TransactionDetailViewModel> Details { get; set; } = new List<TransactionDetailViewModel>();
    }

    public class TransactionDetailViewModel
    {
        [Required]
        public int PricelistJasaId { get; set; }

        public string? JasaName { get; set; } // Hanya untuk display jika diperlukan saat postback error

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Qty harus lebih dari 0")]
        public decimal Qty { get; set; }

        [Required]
        public decimal PriceAtTime { get; set; } // Harga dari database (menghindari manipulasi form)

        public decimal Discount { get; set; } = 0; // Diskon numerik/rupiah per item
    }
}
