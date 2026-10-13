using System;
using System.Collections.Generic;
using System.Text;

namespace MiniApi.Models;

//Entidades 
public record Order
{
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public List<OrderItem> Items { get; init; } = new();
    public decimal Total { get; init; }
    public string Estado { get; init; } = OrderStatus.Pendiente;
    public DateTime FechaCreacion { get; init; }
}

public record OrderItem
{
    public Guid ProductoId { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
}

// Requests / Responses 
public record CreateOrderRequest(Guid UsuarioId, List<CreateOrderItemRequest>? Items);

public record CreateOrderItemRequest(Guid ProductoId, int Cantidad);

public record UpdateOrderStatusRequest(string? Estado);

public record UpdateOrderStatusResponse(Guid Id, string Estado, DateTime FechaActualizacion);

// Lo que Orders necesita leer de Products API (GET /api/products/{id}) ──
public record ProductInfo(Guid Id, string Nombre, decimal Precio, int Stock);

// Estados y transiciones 
// OJO: la consigna define los 5 estados pero NO las transiciones válidas.

public static class OrderStatus
{
    public const string Pendiente = "Pendiente";
    public const string Confirmada = "Confirmada";
    public const string Enviada = "Enviada";
    public const string Entregada = "Entregada";
    public const string Cancelada = "Cancelada";

    public static readonly string[] All = [Pendiente, Confirmada, Enviada, Entregada, Cancelada];

    private static readonly Dictionary<string, string[]> Transitions = new()
    {
        [Pendiente] = [Confirmada, Cancelada],
        [Confirmada] = [Enviada, Cancelada],
        [Enviada] = [Entregada],
        [Entregada] = [],
        [Cancelada] = []
    };

    /// <summary>Valida el texto recibido (sin distinguir mayúsculas) y lo devuelve normalizado.</summary>
    public static bool TryNormalize(string? input, out string estado)
    {
        estado = All.FirstOrDefault(s => string.Equals(s, input?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        return estado.Length > 0;
    }

    public static bool CanTransition(string from, string to) =>
        Transitions.TryGetValue(from, out var next) && next.Contains(to);
}