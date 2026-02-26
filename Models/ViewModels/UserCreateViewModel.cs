using System.ComponentModel.DataAnnotations;

namespace LaundryManagement.Models.ViewModels
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Nama wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama tidak boleh lebih dari {1} karakter.")]
        [Display(Name = "Nama Lengkap")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username wajib diisi.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username harus antara {2} dan {1} karakter.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor Telepon wajib diisi.")]
        [Phone(ErrorMessage = "Format Nomor Telepon tidak valid.")]
        [Display(Name = "Nomor Telepon")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password minimal {2} karakter.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konfirmasi Password wajib diisi.")]
        [DataType(DataType.Password)]
        [Display(Name = "Konfirmasi Password")]
        [Compare("Password", ErrorMessage = "Password dan Konfirmasi Password tidak cocok.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role wajib dipilih.")]
        [Display(Name = "Role Akses")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Foto wajib diunggah.")]
        [Display(Name = "Foto Profil")]
        public IFormFile? ProfilePicture { get; set; }
    }
}
