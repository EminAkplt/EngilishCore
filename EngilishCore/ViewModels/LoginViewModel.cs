using System.ComponentModel.DataAnnotations;

namespace EnglishCore.ViewModels
{
    // Giriş formunun arkasındaki ViewModel. Sadece kimlik doğrulama için gerekli iki alan.
    public class LoginViewModel
    {
        // Kullanıcının giriş yaparken kullanacağı kullanıcı adı
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [MaxLength(50)]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        // Kullanıcının giriş şifresi
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [MaxLength(100)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;
    }
}
