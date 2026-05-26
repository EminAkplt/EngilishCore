namespace EnglishCore.Models
{
    // MVC şablonundan gelen, hata sayfasında gösterilen RequestId bilgisini tutan view model
    public class ErrorViewModel
    {
        // İsteğin izlenebilir kimliği (Activity.Current.Id veya HttpContext.TraceIdentifier)
        public string? RequestId { get; set; }

        // RequestId doluysa view'da göster, boşsa gizle
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
