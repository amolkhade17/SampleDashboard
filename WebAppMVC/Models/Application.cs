namespace WebAppMVC.Models
{
    public class Application
    {
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public string RouteUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}