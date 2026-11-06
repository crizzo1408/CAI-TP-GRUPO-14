using MiniApi.Models;

namespace MiniApi.Extensions.Endpoints
{
    // ⚠ SOLO PARA PROBAR SIN Users NI Products.
    // Antes de entregar: borrar este archivo, la línea app.MapOrdersMockEndpoints()
    // y poner UsarServiciosSimulados = false en OrderEndpointsExtension.cs
    public static class OrdersMockEndpoints
    {
        private static readonly string[] UsuariosExistentes = new string[]
        {
            "a1b2c3d4-0000-0000-0000-111122223333", // María (ejemplo de la consigna)
            "b2b2b2b2-0000-0000-0000-222222222222"  // segundo usuario para probar el filtro
        };

        private static readonly ProductInfo[] Productos = new ProductInfo[]
        {
            new ProductInfo(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "Notebook Dell XPS 15", 1500.00m, 10),
            new ProductInfo(Guid.Parse("aaaabbbb-cccc-dddd-eeee-ffff00001111"), "Mouse Logitech", 25.50m, 2),
            new ProductInfo(Guid.Parse("11111111-2222-3333-4444-555555555555"), "Auriculares Sony", 80.00m, 0)
        };

        // Este id simula que Products API se cae (sirve para probar ORD-007)
        private static readonly Guid ProductoQueFalla = Guid.Parse("99999999-9999-9999-9999-999999999999");

        public static void MapOrdersMockEndpoints(this WebApplication app)
        {
            app.MapGet("/mock/api/users/{id}", MockGetUser)
                .WithTags("Mock (borrar antes de entregar)");

            app.MapGet("/mock/api/products/{id}", MockGetProduct)
                .WithTags("Mock (borrar antes de entregar)");
        }

        private static IResult MockGetUser(string id)
        {
            foreach (string usuario in UsuariosExistentes)
            {
                if (string.Equals(usuario, id, StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Ok(new { id = usuario, nombre = "Usuario", apellido = "De Prueba" });
                }
            }
            return Results.NotFound();
        }

        private static IResult MockGetProduct(string id)
        {
            Guid productoId;
            if (!Guid.TryParse(id, out productoId))
            {
                return Results.NotFound();
            }

            if (productoId == ProductoQueFalla)
            {
                return Results.StatusCode(500);
            }

            foreach (ProductInfo producto in Productos)
            {
                if (producto.Id == productoId)
                {
                    return Results.Ok(producto);
                }
            }
            return Results.NotFound();
        }
    }
}