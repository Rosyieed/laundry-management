using System.ComponentModel.DataAnnotations;

namespace LaundryManagement.Models.ViewModels
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama tidak boleh lebih dari {1} karakter.")]
        [Display(Name = "Nama Lengkap")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username wajib diisi.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username minimal {2} dan maksimal {1} karakter.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor Telepon wajib diisi.")]
        [Phone(ErrorMessage = "Format Nomor Telepon tidak valid.")]
        [StringLength(20, ErrorMessage = "Nomor Telepon tidak boleh lebih dari {1} karakter.")]
        [Display(Name = "Nomor Telepon")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password minimal {2} karakter.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password Baru (Opsional)")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Konfirmasi Password Baru")]
        [Compare("Password", ErrorMessage = "Password dan Konfirmasi Password tidak cocok.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Role wajib dipilih.")]
        [Display(Name = "Role Akses")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Foto Profil")]
        public Microsoft.AspNetCore.Http.IFormFile? ProfilePicture { get; set; }
    }
}
