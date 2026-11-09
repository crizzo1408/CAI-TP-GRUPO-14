namespace MiniApi.Extensions.Endpoints
{
    // ⚠ SOLO PARA PROBAR mientras Users.API no tenga GET /api/users/{id}.
    // Simula ÚNICAMENTE la validación de usuario. Products se usa real.
    // Cuando Users tenga el endpoint: borrar este archivo, el if que llama a
    // MapOrdersMockEndpoints() y poner SimularUsers = false en OrderEndpointsExtension.cs
    public static class OrdersMockEndpoints
    {
        private static readonly string[] UsuariosExistentes = new string[]
        {
            "a1b2c3d4-0000-0000-0000-111122223333", // María (ejemplo de la consigna)
            "b2b2b2b2-0000-0000-0000-222222222222"  // segundo usuario para probar el filtro
        };

        public static void MapOrdersMockEndpoints(this WebApplication app)
        {
            app.MapGet("/mock/api/users/{id}", MockGetUser)
                .WithTags("Mock (borrar antes de entregar)");
        }

        // 200 si el usuario está en la lista, 404 si no (así se sigue pudiendo probar ORD-003)
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
    }
}