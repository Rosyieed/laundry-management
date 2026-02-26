using System.ComponentModel.DataAnnotations;

namespace LaundryManagement.Models.ViewModels
{
    public class JasaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama Jasa wajib diisi")]
        public string NamaJasa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Satuan wajib dipilih")]
        [RegularExpression("^(Kg|Pcs|M|M2)$", ErrorMessage = "Pilihan satuan tidak tidak valid")]
        public string Satuan { get; set; } = string.Empty;
    }
}
