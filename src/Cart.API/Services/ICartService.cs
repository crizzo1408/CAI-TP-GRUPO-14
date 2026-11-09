using Cart.API.DTOs;

namespace Cart.API.Services;

public interface ICartService
{
    Task<CartResponse> GetAsync(string userId);
    Task<CartResponse> AddItemAsync(string userId, AddCartItemRequest request);
    Task<CartResponse> UpdateItemAsync(string userId, string productId, UpdateCartItemRequest request);
    Task RemoveItemAsync(string userId, string productId);
    Task ClearAsync(string userId);
}
