using System.ComponentModel.DataAnnotations;
using LaundryManagement.Models.Entities;

namespace LaundryManagement.Models.ViewModels
{
    public class TransactionEditViewModel
    {
        public int Id { get; set; }

        public string InvoiceNo { get; set; } = null!;

        // Status saat ini di database (digunakan untuk validasi backend agar user tidak bypass rules)
        public string CurrentStatus { get; set; } = "Pending";

        [Required]
        public string Status { get; set; } = "Pending"; // Status baru yang dipilih

        public int CustomerId { get; set; }

        // Hanya untuk display, edit pelanggan dari halaman edit tidak diperbolehkan sesuai spek (atau bisa jika diperlukan, tapi cart fokus pada detail item/status)
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";

        [Required(ErrorMessage = "Tanggal order wajib diisi")]
        public DateTime OrderDate { get; set; }

        public DateTime? EstimatedFinishDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Unpaid";

        [Required]
        [MaxLength(20)]
        public string PaymentMethod { get; set; } = "Cash";

        public string? Notes { get; set; }

        // List Detail Jasa untuk diedit atau hanya sebagai view tergantung access rules
        public List<TransactionDetailViewModel> Details { get; set; } = new List<TransactionDetailViewModel>();
    }
}
