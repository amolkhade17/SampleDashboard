namespace WebAppMVC.Models
{
    public class CrudOperation
    {
        public int OperationId { get; set; }
        public int ApplicationId { get; set; }
        public string OperationName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}