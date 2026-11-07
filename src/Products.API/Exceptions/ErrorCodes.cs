namespace MiniApi.Exceptions;

public static class ErrorCodes
{
    public const string ProductNotFound = "PRD-001";
    public const string InvalidProduct = "PRD-002";
    public const string DuplicateProduct = "PRD-003";
    public const string ProductWithActiveOrders = "PRD-004";
    public const string InternalProductError = "PRD-005";
}