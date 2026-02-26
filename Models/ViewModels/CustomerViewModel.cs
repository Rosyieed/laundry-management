using System.ComponentModel.DataAnnotations;

namespace LaundryManagement.Models.ViewModels
{
    public class CustomerViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama Pelanggan wajib diisi")]
        [MaxLength(100, ErrorMessage = "Nama Pelanggan maksimal 100 karakter")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor Telepon wajib diisi")]
        [MaxLength(15, ErrorMessage = "Nomor Telepon maksimal 15 karakter")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Alamat maksimal 255 karakter")]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
