using System.ComponentModel.DataAnnotations;

namespace EnglishCore.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [MaxLength(50)]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifre en az 4 karakter olmalıdır.")]
        [MaxLength(100)]    
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Şifre (Tekrar)")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seviye seçimi zorunludur.")]
        [MaxLength(2)]
        [Display(Name = "Başlangıç Seviyesi")]
        public string SelectedLevel { get; set; } = string.Empty;
    }
}
