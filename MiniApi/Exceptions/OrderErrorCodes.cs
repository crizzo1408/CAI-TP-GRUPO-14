namespace MiniApi.Exceptions;


public static class OrderErrorCodes
{
    
    public const string NotFound = "ORD-001";
    
    public const string InvalidData = "ORD-002";
   
    public const string UserNotFound = "ORD-003";
   
    public const string ProductNotFound = "ORD-004";
    
    public const string InsufficientStock = "ORD-005";
   
    public const string InvalidTransition = "ORD-006";
  
    public const string Internal = "ORD-007";
}