using System.ComponentModel.DataAnnotations;

namespace LaundryManagement.Models.ViewModels
{
    public class PricelistJasaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Jasa wajib dipilih")]
        public int JasaId { get; set; }

        [Required(ErrorMessage = "Tipe Layanan wajib dipilih")]
        public int TipeLayanan { get; set; }

        [Required(ErrorMessage = "Harga wajib diisi")]
        [Range(0, double.MaxValue, ErrorMessage = "Harga tidak boleh kurang dari 0")]
        public decimal Harga { get; set; }
    }
}
