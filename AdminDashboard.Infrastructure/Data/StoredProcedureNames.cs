namespace AdminDashboard.Infrastructure.Data;

/// <summary>
/// Centralized stored procedure name mappings
/// </summary>
public static class StoredProcedureNames
{
    // User Related Procedures
    public static class User
    {
        public const string Authenticate = "SP_AuthenticateUser";
        public const string GetById = "SP_GetUserById";
        public const string GetByUsername = "SP_GetUserByUsername";
        public const string GetAll = "SP_GetAllUsers";
        public const string Create = "SP_CreateUser";
        public const string Update = "SP_UpdateUser";
        public const string Delete = "SP_DeleteUser";
        public const string UpdateStatus = "SP_UpdateUserStatus";
    }

    // Product Related Procedures
    public static class Product
    {
        public const string GetById = "SP_GetProductById";
        public const string GetAll = "SP_GetAllProducts";
        public const string Create = "SP_CreateProduct";
        public const string Update = "SP_UpdateProduct";
        public const string Delete = "SP_DeleteProduct";
        public const string UpdateStock = "SP_UpdateProductStock";
    }

    // Category Related Procedures
    public static class Category
    {
        public const string GetAll = "SP_GetAllCategories";
        public const string Create = "SP_CreateCategory";
    }

    // MakerChecker Related Procedures
    public static class MakerChecker
    {
        public const string GetPendingRecords = "SP_GetPendingRecords";
        public const string GetApprovedRecords = "SP_GetApprovedRecords";
        public const string GetRejectedRecords = "SP_GetRejectedRecords";
        public const string GetRecordById = "SP_GetPendingRecordById";
        public const string CreatePendingRecord = "SP_CreatePendingRecord";
        public const string ApproveRecord = "SP_ApproveRecord";
        public const string RejectRecord = "SP_RejectRecord";
    }

    // Dashboard Related Procedures
    public static class Dashboard
    {
        public const string GetCounts = "SP_GetDashboardCounts";
        public const string GetRecentActivities = "SP_GetRecentActivities";
    }

    // Report Related Procedures
    public static class Report
    {
        public const string GetUserActivity = "SP_GetUserActivityReport";
        public const string GetProductStock = "SP_GetProductStockReport";
        public const string GetMakerCheckerSummary = "SP_GetMakerCheckerSummary";
    }

    // Role Related Procedures
    public static class Role
    {
        public const string GetAll = "SP_GetAllRoles";
    }
}
