using System;
using System.Collections.Generic;
using System.Text;

namespace MiniApi.Models;

//Entidades 


public record Order
{
  
    public Guid Id { get; init; }

    public Guid UsuarioId { get; init; }

    public List<OrderItem> Items { get; init; } = new List<OrderItem>();

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

/// Body de POST /api/orders
public record CreateOrderRequest
{
    
    public Guid UsuarioId { get; init; }

    
    public List<CreateOrderItemRequest>? Items { get; init; }
}


public record CreateOrderItemRequest
{
    
    public Guid ProductoId { get; init; }

    public int Cantidad { get; init; }
}

/// Body de PUT /api/orders/{id}/status
public record UpdateOrderStatusRequest
{
    
    public string? Estado { get; init; }
}

public record UpdateOrderStatusResponse(Guid Id, string Estado, DateTime FechaActualizacion);


public record ProductInfo(Guid Id, string Nombre, decimal Precio, int Stock);

//Estados y transiciones
public static class OrderStatus
{
    public const string Pendiente = "Pendiente";
    public const string Confirmada = "Confirmada";
    public const string Enviada = "Enviada";
    public const string Entregada = "Entregada";
    public const string Cancelada = "Cancelada";

    public static readonly string[] All = new string[] { Pendiente, Confirmada, Enviada, Entregada, Cancelada };

    private static readonly Dictionary<string, string[]> Transitions = new Dictionary<string, string[]>
    {
        { Pendiente,  new string[] { Confirmada, Cancelada } },
        { Confirmada, new string[] { Enviada, Cancelada } },
        { Enviada,    new string[] { Entregada } },
        { Entregada,  new string[] { } },
        { Cancelada,  new string[] { } }
    };

    public static bool TryNormalize(string? input, out string estado)
    {
        estado = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string limpio = input.Trim();
        foreach (string s in All)
        {
            if (string.Equals(s, limpio, StringComparison.OrdinalIgnoreCase))
            {
                estado = s;
                return true;
            }
        }
        return false;
    }

    public static bool CanTransition(string from, string to)
    {
        string[]? siguientes;
        if (!Transitions.TryGetValue(from, out siguientes))
        {
            return false;
        }

        foreach (string s in siguientes)
        {
            if (s == to)
            {
                return true;
            }
        }
        return false;
    }
}

